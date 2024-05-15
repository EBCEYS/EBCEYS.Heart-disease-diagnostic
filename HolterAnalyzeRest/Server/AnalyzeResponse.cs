namespace HolterAnalyzeRest.Server
{
    /// <summary>
    /// The analyze response.
    /// </summary>
    public class AnalyzeResponse
    {
        /// <summary>
        /// The exception message.
        /// </summary>
        public string ExceptionMessage { get; set; } = null;
        /// <summary>
        /// The files.
        /// </summary>
        public List<byte[]> Files { get; set; }
        /// <summary>
        /// The result values.
        /// </summary>
        public Dictionary<string, double> Values { get; set; }
    }
}
