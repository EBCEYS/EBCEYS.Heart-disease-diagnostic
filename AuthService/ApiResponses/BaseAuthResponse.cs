namespace AuthService.ApiResponses
{
    /// <summary>
    /// The base api response.
    /// </summary>
    public class BaseAuthResponse
    {
        /// <summary>
        /// The user id.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The username.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The response result.
        /// </summary>
        public UsersResponseResult ResponseResult { get; set; }
    }
}
