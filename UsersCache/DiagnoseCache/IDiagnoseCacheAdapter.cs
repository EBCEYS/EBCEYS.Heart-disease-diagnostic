using DiagnoseDataObjects;

namespace UsersCache.DiagnoseCache
{
    public interface IDiagnoseCacheAdapter
    {
        Task<bool> AddDiagnoseResultsAsync(string requestId, DiagnoseResult data);
        Task<int> GetCurrentDiagnosesCountAsync();
        Task<List<DiagnoseResult>?> GetDiagnoseResultsIfExistsAsync(string requestId);
        Task<bool> RemoveDiagnoseResultsDataAsync(string id);
    }
}