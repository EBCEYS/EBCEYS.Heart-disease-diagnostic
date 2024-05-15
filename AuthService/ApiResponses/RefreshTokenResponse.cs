namespace AuthService.ApiResponses
{
    /// <summary>
    /// The refresh token api reponse.
    /// </summary>
    public class RefreshTokenResponse : BaseAuthResponse
    {
        /// <summary>
        /// The new JWT.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// The new refresh token.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
