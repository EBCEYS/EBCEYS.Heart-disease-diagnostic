using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;

namespace HeartDiseasesDiagnosticExtentions.DataBaseExtensions
{
    public class DBWriteRequest
    {
        public string Id { get; set; }
        public object Request { get; set; }
        public ActionResponse Response { get; set; }
        public DataSetTypes DataSetType { get; set; }
        public string UserId { get; set; }

        public DBWriteRequest() { }

        public DBWriteRequest(string id, object request, ActionResponse response, DataSetTypes dataSetTypes, string userId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"\"{nameof(id)}\" не может быть неопределенным или пустым.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"\"{nameof(userId)}\" не может быть пустым или содержать только пробел.", nameof(userId));
            }

            Id = id;
            DataSetType = dataSetTypes;
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
            UserId = userId;
        }
    }
}
