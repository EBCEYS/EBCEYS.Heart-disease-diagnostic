using CacheAdapters.UsersCache;
using DataBaseObjects.UsersDB;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace JWTExtensions
{
    public class RevokableJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        private readonly IUsersCacheAdapter cacheAdapter;

        public RevokableJwtSecurityTokenHandler(IUsersCacheAdapter cacheAdapter)
        {
            this.cacheAdapter = cacheAdapter ?? throw new ArgumentNullException(nameof(cacheAdapter));
        }
        public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            try
            {
                ClaimsPrincipal claimsPrincipal = base.ValidateToken(token, validationParameters, out validatedToken);
                Claim userName = claimsPrincipal.FindFirst(ClaimTypes.Name);
                Claim[] roles = claimsPrincipal.FindAll(ClaimTypes.Role).ToArray();
                Claim id = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
                if (!CheckUsersParams(id, userName, roles))
                {
                    throw new SecurityTokenValidationException();
                }
                return claimsPrincipal;
            }
            catch(SecurityTokenExpiredException ex)
            {
                JwtSecurityToken jwt = base.ReadJwtToken(token);
                Claim id = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (id != null)
                {
                    cacheAdapter.RemoveUserFromCache(id.Value);
                }
                throw ex;
            }
        }

        private bool CheckUsersParams(Claim usrId, Claim usrName, Claim[] usrRoles)
        {
            try
            {
                string id = usrId.Value;
                string name = usrName.Value;
                List<string> roles = new();
                usrRoles.AsParallel().ForAll(x =>
                {
                    roles.Add(x.Value);
                });

                User usr = cacheAdapter.GetUserFromCache(id);
                bool nameCompare = string.Compare(usr.Name, name, StringComparison.OrdinalIgnoreCase) == 0;
                bool idCompare = string.Compare(usr.Id, id, StringComparison.OrdinalIgnoreCase) == 0;
                bool rolesCompare = CompareRoles(usr.Role!.Roles!.ToArray(), roles.ToArray());

                return nameCompare
                    && idCompare
                    && rolesCompare;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static bool CompareRoles(string[] item1, string[] item2)
        {
            if (item1.Length != item2.Length)
            {
                return false;
            }
            bool res = true;
            foreach (string item in item1)
            {
                res &= item2.Contains(item);
            }
            return res;
        }
    }
}
