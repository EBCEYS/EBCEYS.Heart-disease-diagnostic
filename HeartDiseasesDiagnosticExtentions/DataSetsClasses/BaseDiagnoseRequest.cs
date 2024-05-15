using System.Text.Json.Serialization;

namespace HeartDiseasesDiagnosticExtentions.DataSetsClasses
{
    public class BaseDiagnoseRequest
    {
        public object Params { get; set; }
        public string Method { get; set; }

    }
}
