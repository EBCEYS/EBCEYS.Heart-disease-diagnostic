using DataBaseObjects.RolesDB;

namespace AuthService.ApiResponses
{
    /// <summary>
    /// The response on login request.
    /// </summary>
    public class LoginResponse : BaseAuthResponse
    {
        /// <summary>
        /// The generated token.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// The roles.
        /// </summary>
        public UserRole Role { get; set; }
        /// <summary>
        /// The refresh token.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
