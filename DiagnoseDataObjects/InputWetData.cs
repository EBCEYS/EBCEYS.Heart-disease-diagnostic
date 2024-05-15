using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace DiagnoseDataObjects
{
    /// <summary>
    /// Data to diagnose.
    /// </summary>
    public class InputWetData
    {
        /// <summary>
        /// The data type.
        /// </summary>
        [Required]
        public DataSetTypes DataType { get; set; }
        /// <summary>
        /// The session id.
        /// </summary>
        [Required]
        public string? SessionId { get; set; }
        /// <summary>
        /// The data to diagnose.
        /// </summary>
        [Required]
        public JObject? DataToDiagnose { get; set; }
    }
}