using CacheAdapters.CacheModels;
using Redis.OM;
using Redis.OM.Searching;
using UsersCache;

namespace CacheAdapters.JwtCache
{
    public class JwtCacheAdapter : IJwtCacheAdapter
    {
        private readonly RedisCollection<JwtCachedData> data;
        public JwtCacheAdapter(CacheAdapterSettings settings)
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
            provider.Connection.CreateIndex(typeof(JwtCachedData));
            data = (RedisCollection<JwtCachedData>)provider.RedisCollection<JwtCachedData>();
        }

        public async Task AddJwtDataAsync(JwtCachedData jwtData)
        {
            JwtCachedData? currentData = await data.FindByIdAsync(jwtData.RefreshToken);
            if (currentData == null)
            {
                await data.InsertAsync(jwtData);
            }
            await data.UpdateAsync(jwtData);
        }

        public async Task<JwtCachedData?> GetJwtDataAsync(string refreshToken)
        {
            return await data.FindByIdAsync(refreshToken) ?? null;
        }

        public async Task<bool> RemoveJwtDataAsync(string refreshToken)
        {
            JwtCachedData? currentData = await data.FindByIdAsync(refreshToken);
            if (currentData == null)
            {
                return false;
            }
            await data.DeleteAsync(currentData);
            return true;
        }
        public async Task<bool> RemoveJwtDataByUserAsync(string userId)
        {
            IList<JwtCachedData>? currentData = await data.Where(x => x.UserId == userId).ToListAsync();
            if (currentData == null)
            {
                return false;
            }
            await data.DeleteAsync(currentData);
            return true;
        }

        public async Task<List<JwtCachedData>?> GetTimeoutedJwtDataAsync(int countToTake = 100)
        {
            //return (List<JwtCachedData>?)(await data.Where(d => (DateTime.UtcNow > d.DeletionDateTime)).ToListAsync() ?? null);
            List<JwtCachedData>? jwtData = (List<JwtCachedData>?)await data.OrderBy(d => d.DeletionDateTime).Take(countToTake).ToListAsync();
            if (jwtData == null)
            {
                return null;
            }
            return jwtData.Where(d => DateTime.UtcNow > d.DeletionDateTime).ToList();
        }

        public async Task<bool> RemoveJwtDataAsync(params JwtCachedData[] dataToRemove)
        {
            IDictionary<string, JwtCachedData?> currentData = await data.FindByIdsAsync(dataToRemove.Select(d => d.RefreshToken));
            if (currentData != null)
            {
                await data.DeleteAsync(currentData.Select(d => d.Value)!);
                /*currentData.ToList().AsParallel().ForAll(async d =>
                {
                    if (d.Value != null)
                    {
                        await data.DeleteAsync(d.Value!);
                    }
                });*/
                return true;
            }
            return false;
        }

        public async Task<int> GetCurrentCachedJwtInfoCountAsync() => await data.CountAsync();
    }
}
