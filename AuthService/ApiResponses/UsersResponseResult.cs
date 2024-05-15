namespace AuthService.ApiResponses
{
    /// <summary>
    /// The user's response result.
    /// </summary>
    public enum UsersResponseResult
    {
        /// <summary>
        /// The ok result.
        /// </summary>
        OK,
        /// <summary>
        /// The error result.
        /// </summary>
        ERROR,
        /// <summary>
        /// The connection error result.
        /// </summary>
        ERROR_CONNECTION,
        /// <summary>
        /// The user already exists result.
        /// </summary>
        ERROR_USER_ALREADY_EXISTS,
        /// <summary>
        /// The wrong password result.
        /// </summary>
        ERROR_WRONG_PASSWORD,
        /// <summary>
        /// The role does not exists result.
        /// </summary>
        ERROR_ROLE_DOES_NOT_EXISTS,
        /// <summary>
        /// The user does not exists result.
        /// </summary>
        ERROR_USER_DOES_NOT_EXISTS,
        /// <summary>
        /// The wrong role result.
        /// </summary>
        ERROR_WRONG_ROLE,
        /// <summary>
        /// The wrong params result.
        /// </summary>
        ERROR_WRONG_PARAMS,
        /// <summary>
        /// The alerts are not exists.
        /// </summary>
        ALERTS_ARE_NOT_EXISTS,
        /// <summary>
        /// The refresh token is expired or not exists.
        /// </summary>
        REFRESH_TOKEN_IS_EXPIRED_OR_DOES_NOT_EXISTS,
        /// <summary>
        /// The user is not authorized.
        /// </summary>
        USER_IS_NOT_AUTHORIZED
    }
}
