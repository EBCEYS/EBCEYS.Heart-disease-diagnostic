using CacheAdapters.CacheModels;

namespace CacheAdapters.JwtCache
{
    public interface IJwtCacheAdapter
    {
        Task AddJwtDataAsync(JwtCachedData jwtData);
        Task<int> GetCurrentCachedJwtInfoCountAsync();
        Task<JwtCachedData?> GetJwtDataAsync(string refreshToken);
        Task<List<JwtCachedData>?> GetTimeoutedJwtDataAsync(int countToTake = 100);
        Task<bool> RemoveJwtDataAsync(string refreshToken);
        Task<bool> RemoveJwtDataAsync(params JwtCachedData[] dataToRemove);
        Task<bool> RemoveJwtDataByUserAsync(string userId);
    }
}