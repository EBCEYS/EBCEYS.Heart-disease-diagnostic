namespace AuthService.ApiResponses
{
    /// <summary>
    /// The response results types.
    /// </summary>
    public enum ResponseResults
    {
        /// <summary>
        /// The OK result.
        /// </summary>
        OK,
        /// <summary>
        /// The error result.
        /// </summary>
        ERROR,
        /// <summary>
        /// The error result if user already exists.
        /// </summary>
        ERROR_USER_ALREADY_EXISTS
    }
}
