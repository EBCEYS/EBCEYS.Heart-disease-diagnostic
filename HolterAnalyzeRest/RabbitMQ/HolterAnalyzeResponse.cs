namespace HolterAnalyzeRest.RabbitMQ
{
    /// <summary>
    /// The holter analyze rabbitMQ response.
    /// </summary>
    public class HolterAnalyzeResponse
    {
        /// <summary>
        /// The request/response id.
        /// </summary>
        public string RequestId { get; set; }
        /// <summary>
        /// The files.
        /// </summary>
        public List<byte[]> Files { get; set; }
        /// <summary>
        /// The values.
        /// </summary>
        public Dictionary<string, double> Values { get; set; }
    }
}
