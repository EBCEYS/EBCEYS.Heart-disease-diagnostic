using DataBaseObjects.AlertDB;
using DataBaseObjects.HDDSDBContext;
using DataBaseObjects.RolesDB;
using DataBaseObjects.UsersDB;
using HeartDiseasesDiagnosticExtentions.AuthExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UsersContracts;

namespace UsersDBAdapter.DataBaseRepository
{
    public class UsersDBRepository : IUsersDBRepository
    {
        private readonly ILogger<UsersDBRepository> logger;
        private readonly DiagnoseDBContext dBContext;

        public UsersDBRepository(ILogger<UsersDBRepository> logger, DiagnoseDBContext dBContext)
        {
            this.logger = logger;
            this.dBContext = dBContext;
        }
        private static User HashUser(User user)
        {
            user.Id = CreateHash($"{user.Name}{user.Password}{user.IsActive}{user.Organization ?? "org"}{user.Email ?? "email"}{Guid.NewGuid()}");
            user.Password = HashPassword(user.Password);
            return user;
        }

        private static string HashPassword(string password)
        {
            return CreateHash($"salt_{password}_ebcey's_salt");
        }

        private static string CreateHash(string input)
        {
            byte[] hashed = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashed);
        }

        public virtual async Task<UsersContractResponse<User>> GetUserAsync(string id)
        {
            try
            {
                logger.LogDebug("Gets user by id: {id}", id);
                User usr = await dBContext.FindAsync<User>(id);
                if (usr == null)
                {
                    return new UsersContractResponse<User>()
                    {
                        Result = UsersResponseContractResult.UserNotFound
                    };
                }
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.Ok,
                    Object = usr
                };
            } 
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting user by id!");
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }
        public virtual async Task<UsersContractResponse<User>> GetUserByNameAndPasswordAsync(string name, string password)
        {
            try
            {
                string pas = HashPassword(password);
                logger.LogDebug("Gets user by name and password: {name}", name);
                User user = await dBContext.Users.Include(u => u.Role).FirstOrDefaultAsync(x => x.Name == name && x.Password == pas && x.IsActive);
                if (user == null)
                {
                    return new UsersContractResponse<User>()
                    {
                        Result = UsersResponseContractResult.UserNotFound
                    };
                }
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.Ok,
                    Object = user
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting user by name and password!");
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }

        public virtual async Task<UsersContractResponse<User>> AddUserAsync(User user)
        {
            try
            {
                User existingUser = await dBContext.Users.FirstOrDefaultAsync(x => x.Name == user.Name);
                if (existingUser != null)
                {
                    return new UsersContractResponse<User>()
                    {
                        Result = UsersResponseContractResult.UserAlreadyExists
                    };
                }
                user = HashUser(user);
                UserRole role = await dBContext.Roles.FindAsync(user.Role.RoleName);
                if (role == null)
                {
                    return new()
                    {
                        Result = UsersResponseContractResult.RoleDoesNotExist
                    };
                }
                user.Role = role;
                logger.LogDebug("Create user with id {id}", user.Id);
                await dBContext.AddAsync(user);
                if (await dBContext.SaveChangesAsync() > 0)
                {
                    return new UsersContractResponse<User>()
                    {
                        Result = UsersResponseContractResult.Ok,
                        Object = user
                    };
                }
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on adding user!");
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }
        public virtual async Task<UsersContractResponse<User>> RemoveUserAsync(string userId)
        {
            try
            {
                logger.LogDebug("Try to remove user with id {id}", userId);
                User usr = await dBContext.FindAsync<User>(userId);
                if (usr != null)
                {
                    usr.IsActive = false;
                    await dBContext.SaveChangesAsync();
                    logger.LogDebug("Remove user: {userId}", userId);
                    return new UsersContractResponse<User>()
                    {
                        Result = UsersResponseContractResult.Ok,
                        Object = usr
                    };
                }
                logger.LogWarning("User {userId} is not found!", userId);
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.UserNotFound
                };
            }
            catch(Exception ex) 
            {
                logger.LogError(ex, "Error on removing user!");
                return new UsersContractResponse<User>()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }
        public virtual async Task<UsersContractResponse<List<User>>> GetAllUsersAsync()
        {
            try
            {
                List<User> users = await dBContext.Users.ToListAsync();
                return new UsersContractResponse<List<User>>()
                {
                    Result = users != null ? UsersResponseContractResult.Ok : UsersResponseContractResult.UserNotFound,
                    Object = users ?? null
                };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on getting all users!");
                return new UsersContractResponse<List<User>>()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
            
        }
        public virtual async Task<UsersContractResponse<User>> UpdateUserRolesAsync(string userId, string newRole)
        {
            try
            {
                logger.LogDebug("Try to update user roles user with id {id}", userId);
                UserRole role = await dBContext.Roles.FindAsync(newRole);
                if (role == null)
                {
                    return new()
                    {
                        Result = UsersResponseContractResult.RoleDoesNotExist
                    };
                }
                User usr = await dBContext.Users.FindAsync(userId);
                if (usr != null && usr.IsActive)
                {
                    usr.Role = role;
                    await dBContext.SaveChangesAsync();
                    logger.LogDebug("Update user: {userId}", userId);
                    return new()
                    {
                        Result = UsersResponseContractResult.Ok,
                        Object = usr
                    };
                }
                logger.LogWarning("User {userId} is not found!", userId);
                return new UsersContractResponse<User>()
                { Result = UsersResponseContractResult.UserNotFound };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on updating user's roles!");
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }
        public virtual async Task<UsersContractResponse<User>> GetUserByNameAsync(string userName)
        {
            try
            {
                User user = await dBContext.Users.FirstOrDefaultAsync(x => x.Name == userName);
                if (user == null)
                {
                    return new()
                    {
                        Result = UsersResponseContractResult.UserNotFound
                    };
                }
                return new()
                {
                    Result = UsersResponseContractResult.Ok,
                    Object = user
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting user by name!");
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }
        public virtual async Task<UsersContractResponse<List<Alert>>> GetUserAlertsAsync(string userId)
        {
            try
            {
                List<Alert> alerts = await dBContext.Alerts.Where(x => x.UserId == userId).ToListAsync() ?? new();
                return new()
                {
                    Result = UsersResponseContractResult.Ok,
                    Object = alerts
                };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on getting user's alerts!");
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }
        public virtual async Task<UsersContractResponse<object>> ChangeUsersPasswordAsync(ChangePasswordApiModel model)
        {
            try
            {
                User user = await dBContext.Users.FirstOrDefaultAsync(u => u.Id == model.UserId && u.Password == HashPassword(model.CurrentPassword));
                if (user == null)
                {
                    return new()
                    {
                        Result = UsersResponseContractResult.UserNotFound
                    };
                }
                user.Password = HashPassword(model.NewPassword);
                await dBContext.SaveChangesAsync();
                return new()
                {
                    Result = UsersResponseContractResult.Ok
                };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on changing user's password!");
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }

        public virtual async Task<UsersContractResponse<object>> CreateRoleAsync(UserRole role)
        {
            try
            {
                UserRole existingRole = await dBContext.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == role.RoleName.ToLower());
                if (existingRole != null)
                {
                    return new()
                    {
                        Result = UsersResponseContractResult.RoleAlreadyExists
                    };
                }
                await dBContext.Roles.AddAsync(role);
                await dBContext.SaveChangesAsync();
                return new()
                {
                    Result = UsersResponseContractResult.Ok
                };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on creating new role! {@role}", role);
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }

        public virtual async Task<UsersContractResponse<List<UserRole>>> GetUserRolesListAsync()
        {
            try
            {
                List<UserRole> roles = await dBContext.Roles.ToListAsync();
                return new()
                {
                    Result = UsersResponseContractResult.Ok,
                    Object = roles ?? new()
                };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on getting roles list!");
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }

        public virtual async Task<UsersContractResponse<object>> RemoveRoleAsync(string roleName)
        {
            try
            {
                UserRole role = await dBContext.Roles.FindAsync(roleName);
                if (role != null)
                {
                    dBContext.Remove(role);
                    await dBContext.SaveChangesAsync();
                    return new()
                    {
                        Result = UsersResponseContractResult.Ok
                    };
                }
                return new()
                {
                    Result = UsersResponseContractResult.RoleDoesNotExist
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on deleting role!");
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }

        public virtual async Task<UsersContractResponse<object>> UpdateRoleAsync(UserRole role)
        {
            try
            {
                UserRole currentRole = await dBContext.Roles.FindAsync(role.RoleName);
                if (currentRole != null)
                {
                    currentRole.Roles = role.Roles;
                    await dBContext.SaveChangesAsync();
                    return new()
                    {
                        Result = UsersResponseContractResult.Ok
                    };
                }
                return new()
                {
                    Result = UsersResponseContractResult.RoleDoesNotExist
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on updating role!");
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
        }
    }
}
