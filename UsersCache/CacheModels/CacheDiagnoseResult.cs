using DiagnoseDataObjects;
using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace CacheAdapters.CacheModels
{
    public class CacheDiagnoseResult
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
        public CacheDiagnoseResult(DiagnoseResult result)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            this.SessionId = result.SessionId;
            this.ResultValue = result.ResultValue;
            this.DataType = result.DataType;
            this.UserId = result.UserId;
            this.DiagnoseData = JsonSerializer.SerializeToDocument(result.DiagnoseData!.ToString());
        }
        public DiagnoseResult ToDiagnoseResultObject()
        {
            return new()
            {
                DataType = this.DataType,
                SessionId = this.SessionId,
                ResultValue = this.ResultValue,
                Result = this.Result,
                DiagnoseData = JObject.Parse(this.DiagnoseData!.RootElement.ToString())
            };
        }
        public CacheDiagnoseResult()
        {

        }
    }
}
