using HolterAnalyzeRest.RabbitMQ;
using NLog;

namespace HolterAnalyzeRest.Server
{
    /// <summary>
    /// The data server.
    /// </summary>
    public class DataServer
    {
        private readonly Logger logger;
        private readonly IConfiguration config;
        /// <summary>
        /// Creates the data server.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The config.</param>
        public DataServer(Logger logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;

            //TODO: добавить отправку байтов файла по rabbitMQ
            
        }

        /// <summary>
        /// Uploads the data async.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="userName">The username.</param>
        /// <returns>The analyze response.</returns>
        public async Task<AnalyzeResponse> UploadDataAsync(MemoryStream stream, string fileName, string userName)
        {
            byte[] bytes = stream.ToArray();
            //string byteStr = Convert.ToBase64String(bytes);


            HolterAnalyzeRequest request = new(bytes, fileName, userName);

            //Test values!
            Dictionary<string, double> values = new()
            {
                { "key1", 0.1516 }
            };

            await Task.Delay(TimeSpan.FromSeconds(5));

            return new()
            {
                Files = new()
                {
                    bytes
                },
                Values = values
            };

            //TODO: добавить отправку байтов файла по rabbitMQ
        }
    }
}
