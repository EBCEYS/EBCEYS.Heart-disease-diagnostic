using DataBaseObjects.RolesDB;

namespace AuthService.ApiResponses
{
    /// <summary>
    /// The registration response.
    /// </summary>
    public class RegistrationResponse : BaseAuthResponse
    {
        /// <summary>
        /// The user's role.
        /// </summary>
        public UserRole Role { get; set; }
    }
}
