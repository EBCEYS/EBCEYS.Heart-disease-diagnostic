using StackExchange.Redis;
using System.Text.Json;

namespace DiagnoseDataCache
{
    public class DiagnoseCacheAdapter
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase database;
        private readonly JsonSerializerOptions serializerOptions;
        public DiagnoseCacheAdapter(CacheAdapterSettings settings, JsonSerializerOptions? serializerOptions = null)
        {
                
        }
    }
}