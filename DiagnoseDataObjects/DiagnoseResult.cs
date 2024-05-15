using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Newtonsoft.Json.Linq;

namespace DiagnoseDataObjects
{
    /// <summary>
    /// The diagnose result.
    /// </summary>
    public class DiagnoseResult
    {
        /// <summary>
        /// The session id.
        /// </summary>
        public string? SessionId { get; set; }
        /// <summary>
        /// The diagnose result value.
        /// </summary>
        public double? ResultValue { get; set; }
        /// <summary>
        /// The diagnose result.
        /// </summary>
        public Result? Result { get; set; }
        /// <summary>
        /// The data type.
        /// </summary>
        public DataSetTypes? DataType { get; set; }
        /// <summary>
        /// The user id.
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// The diagnose data.
        /// </summary>
        public JObject? DiagnoseData { get; set; }
    }
}
