using DataBaseObjects.AlertDB;
using DataBaseObjects.RolesDB;
using DataBaseObjects.UsersDB;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using HeartDiseasesDiagnosticExtentions.AuthExtensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersContracts;
using UsersDBAdapter.DataBaseRepository;

namespace UsersDBAdapter.RabbitMQControllers
{
    public class RabbitMQUsersDBAdapterController : RabbitMQControllerBase
    {
        private readonly ILogger<RabbitMQUsersDBAdapterController> logger;
        private readonly IUsersDBRepository dBRepository;

        public RabbitMQUsersDBAdapterController(ILogger<RabbitMQUsersDBAdapterController> logger, IUsersDBRepository dBRepository)
        {
            this.logger = logger;
            this.dBRepository = dBRepository;
        }

        [RabbitMQMethod("AddUser")]
        public async Task<UsersContractResponse<User>> AddUserAsync(User user)
        {
            return await dBRepository.AddUserAsync(user);
        }
        [RabbitMQMethod("GetUser")]
        public async Task<UsersContractResponse<User>> GetUserAsync(string id)
        {
            return await dBRepository.GetUserAsync(id);
        }
        [RabbitMQMethod("LoginUser")]
        public async Task<UsersContractResponse<User>> LoginUser(LoginModel model)
        {
            return await dBRepository.GetUserByNameAndPasswordAsync(model.UserName, model.Password);
        }
        [RabbitMQMethod("RemoveUser")]
        public async Task<UsersContractResponse<User>> RemoveUser(string id)
        {
            return await dBRepository.RemoveUserAsync(id);
        }
        [RabbitMQMethod("GetAllUsers")]
        public async Task<UsersContractResponse<List<User>>> GetAllUsers()
        {
            return await dBRepository.GetAllUsersAsync();
        }
        [RabbitMQMethod("UpdateUserRoles")]
        public async Task<UsersContractResponse<User>> UpdateUserRoles(string userId, string newRole)
        {
            return await dBRepository.UpdateUserRolesAsync(userId, newRole);
        }
        [RabbitMQMethod("Ping")]
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        public async Task<string> Ping()
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            logger.LogDebug("Get ping request!");
            return "pong";
        }
        [RabbitMQMethod("GetUserByName")]
        public async Task<UsersContractResponse<User>> GetUserByName(string userName)
        {
            logger.LogDebug("Get userName request: {userName}", userName);
            return await dBRepository.GetUserByNameAsync(userName);
        }
        [RabbitMQMethod("GetUserAlerts")]
        public async Task<UsersContractResponse<List<Alert>>> GetUserAlerts(string userId)
        {
            logger.LogDebug("Get GetUserAlerts request: {userId}", userId);
            return await dBRepository.GetUserAlertsAsync(userId);
        }
        [RabbitMQMethod("ChangePassword")]
        public async Task<UsersContractResponse<object>> ChangeUsersPassword(ChangePasswordApiModel model)
        {
            logger.LogDebug("Get ChangePassword request: {userid}", model.UserId ?? "unknown");
            return await dBRepository.ChangeUsersPasswordAsync(model);
        }
        [RabbitMQMethod("CreateNewRole")]
        public async Task<UsersContractResponse<object>> CreateNewRole(UserRole role) //сделать проверку на существование ролей где-нибудь
        {
            logger.LogDebug("Get CreateNewRole request: {@newRole}", role);
            if (string.IsNullOrEmpty(role.RoleName) || role.Roles == null || !role.Roles.Any())
            {
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
            return await dBRepository.CreateRoleAsync(role);
        }
        [RabbitMQMethod("GetRolesList")]
        public async Task<UsersContractResponse<List<UserRole>>> GetRolesList()
        {
            return await dBRepository.GetUserRolesListAsync();
        }
        [RabbitMQMethod("RemoveRole")]
        public async Task<UsersContractResponse<object>> RemoveRole(string roleName)
        {
            return await dBRepository.RemoveRoleAsync(roleName);
        }
        [RabbitMQMethod("UpdateRole")]
        public async Task<UsersContractResponse<object>> UpdateRole(UserRole role)
        {
            if (string.IsNullOrEmpty(role.RoleName) || role.Roles == null || !role.Roles.Any())
            {
                return new()
                {
                    Result = UsersResponseContractResult.Error
                };
            }
            return await dBRepository.UpdateRoleAsync(role);
        }
    }
}
