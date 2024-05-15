using DataBaseObjects.RolesDB;

namespace AuthService.ApiResponses
{
    /// <summary>
    /// The roles list api response.
    /// </summary>
    public class RolesListResponse : BaseAuthResponse
    {
        /// <summary>
        /// The roles list.
        /// </summary>
        public List<UserRole> Roles { get; set; }
    }
}
