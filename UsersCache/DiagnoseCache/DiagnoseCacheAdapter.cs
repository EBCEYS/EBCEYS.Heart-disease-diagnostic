using CacheAdapters.CacheModels;
using DiagnoseDataObjects;
using Redis.OM;
using Redis.OM.Searching;

namespace UsersCache.DiagnoseCache
{
    public class DiagnoseCacheAdapter : IDiagnoseCacheAdapter
    {
        private readonly RedisCollection<DiagnoseResultCacheData> results;
        public DiagnoseCacheAdapter(CacheAdapterSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            RedisConnectionConfiguration config = new()
            {
                Host = settings.HostName!,
                Port = settings.Port ?? 6379,
                Password = settings.Password ?? null,

            };
            RedisConnectionProvider provider = new(config);
            provider.Connection.CreateIndex(typeof(DiagnoseResultCacheData));
            results = (RedisCollection<DiagnoseResultCacheData>)provider.RedisCollection<DiagnoseResultCacheData>();
        }

        public async Task<List<DiagnoseResult>?> GetDiagnoseResultsIfExistsAsync(string requestId)
        {
            DiagnoseResultCacheData? res = await results.FindByIdAsync(requestId);
            if (res == null)
            {
                return null;
            }
            return res.DiagnoseResults.Select(x => x.ToDiagnoseResultObject()).ToList() ?? null;
        }
        public async Task<bool> AddDiagnoseResultsAsync(string requestId, DiagnoseResult data)
        {
            DiagnoseResultCacheData? res = await results.FindByIdAsync(requestId);
            if (res == null)
            {
                string key = await results.InsertAsync(new DiagnoseResultCacheData(data.SessionId!, TimeSpan.FromSeconds(180))//TODO: добавить параметр в конфигурационный файл с временем жизни сущности
                {
                    DiagnoseResults = new() { new(data) }
                });
                return !string.IsNullOrEmpty(key);
            }
            res.DiagnoseResults.Add(new(data));
            await results.SaveAsync();
            return true;

        }
        public async Task<bool> RemoveDiagnoseResultsDataAsync(string id)
        {
            DiagnoseResultCacheData? data = await results.FindByIdAsync(id);
            if (data == null)
            {
                return false;
            }
            await results.DeleteAsync(data);
            return true;
        }

        public async Task<int> GetCurrentDiagnosesCountAsync() => await results.CountAsync();
    }
}