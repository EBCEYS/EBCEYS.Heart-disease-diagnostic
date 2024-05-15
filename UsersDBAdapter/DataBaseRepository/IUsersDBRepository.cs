using DataBaseObjects.AlertDB;
using DataBaseObjects.RolesDB;
using DataBaseObjects.UsersDB;
using HeartDiseasesDiagnosticExtentions.AuthExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using UsersContracts;

namespace UsersDBAdapter.DataBaseRepository
{
    public interface IUsersDBRepository
    {
        Task<UsersContractResponse<User>> AddUserAsync(User user);
        Task<UsersContractResponse<object>> ChangeUsersPasswordAsync(ChangePasswordApiModel model);
        Task<UsersContractResponse<object>> CreateRoleAsync(UserRole role);
        Task<UsersContractResponse<List<User>>> GetAllUsersAsync();
        Task<UsersContractResponse<List<Alert>>> GetUserAlertsAsync(string userId);
        Task<UsersContractResponse<User>> GetUserAsync(string id);
        Task<UsersContractResponse<User>> GetUserByNameAndPasswordAsync(string name, string password);
        Task<UsersContractResponse<User>> GetUserByNameAsync(string userName);
        Task<UsersContractResponse<List<UserRole>>> GetUserRolesListAsync();
        Task<UsersContractResponse<object>> RemoveRoleAsync(string roleName);
        Task<UsersContractResponse<User>> RemoveUserAsync(string userId);
        Task<UsersContractResponse<object>> UpdateRoleAsync(UserRole role);
        Task<UsersContractResponse<User>> UpdateUserRolesAsync(string userId, string newRoles);
    }
}