namespace HolterAnalyzeRest.RabbitMQ
{
    /// <summary>
    /// The rabbitMQ holter analyze request.
    /// </summary>
    public class HolterAnalyzeRequest
    {
        /// <summary>
        /// The file name.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// The username.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The file's byte array (base64).
        /// </summary>
        public byte[] File { get; set; }
        /// <summary>
        /// The request id.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Creates the new holter analyze request to post it to rabbitMQ.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="userName">The user name.</param>
        public HolterAnalyzeRequest(byte[] file, string fileName, string userName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException($"\"{nameof(fileName)}\" не может быть неопределенным или пустым.", nameof(fileName));
            }

            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException($"\"{nameof(userName)}\" не может быть неопределенным или пустым.", nameof(userName));
            }

            File = file ?? throw new ArgumentNullException(nameof(file));
            FileName = fileName;
            UserName = userName;
            RequestId = Guid.NewGuid().ToString();
        }
    }
}
