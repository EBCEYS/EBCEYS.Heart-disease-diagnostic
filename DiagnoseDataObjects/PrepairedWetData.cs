namespace DiagnoseDataObjects
{
    /// <summary>
    /// The prepaired data to post it in queue.
    /// </summary>
    public class PrepairedWetData
    {
        /// <summary>
        /// The input data.
        /// </summary>
        public InputWetData? InputData { get; set; }
        /// <summary>
        /// The user id.
        /// </summary>
        public string? UserId { get; set; }
    }
}
