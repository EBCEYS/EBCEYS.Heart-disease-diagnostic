using System.Text.Json;

namespace HeartDiseasesDiagnosticExtentions.DataSetsClasses
{
    /// <summary>
    /// Класс, используемый для неизвестных системе датасетов.
    /// </summary>
    public class UnknownDataSet : DataSetBase
    {
        public JsonDocument JsonParams { get; set; }
        public string DataSetName { get; set; }
        public UnknownDataSet(string dataSetName, JsonDocument jsonParams)
        {
            DataSetName = dataSetName;
            JsonParams = jsonParams;
            DataSetType = DataSetTypes.Unknown;
        }
        public UnknownDataSet()
        {
            DataSetType = DataSetTypes.Unknown;
        }
    }
}
