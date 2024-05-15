using DiagnoseDataObjects;
using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace DiagnoseRESTService.Models
{
    public class DiagnoseResultRESTService
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
        public JsonDocument? DiagnoseData { get; set; }

        public static DiagnoseResultRESTService CreateFrom(DiagnoseResult data)
        {
            return new DiagnoseResultRESTService()
            {
                SessionId = data.SessionId.Replace($"{data.UserId}_", ""),
                ResultValue = data.ResultValue,
                DataType = data.DataType,
                UserId = data.UserId,
                Result = data.Result,
                DiagnoseData = JsonSerializer.Deserialize<JsonDocument>(data.DiagnoseData.ToString())
            };
        }
    }
}
