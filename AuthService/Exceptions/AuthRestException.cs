using AuthService.ApiResponses;

namespace AuthService.Exceptions
{
    /// <summary>
    /// Исключения для auth-rest-service.
    /// </summary>
    public class AuthRestException : Exception
    {
        /// <summary>
        /// The users response result.
        /// </summary>
        public UsersResponseResult Result { get; private set; }
        /// <summary>
        /// Calls exception with users response result.
        /// </summary>
        /// <param name="result">The user response result.</param>
        public AuthRestException(UsersResponseResult result) : base()
        {
            Result = result;
        }
        /// <summary>
        /// Calls exception with users response result and message.
        /// </summary>
        /// <param name="result">The user response result.</param>
        /// <param name="message">The message.</param>
        public AuthRestException(UsersResponseResult result, string message) : base(message)
        {
            Result = result;
        }
    }
}
