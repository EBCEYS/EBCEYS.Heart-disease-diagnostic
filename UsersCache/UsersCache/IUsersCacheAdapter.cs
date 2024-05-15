using DataBaseObjects.UsersDB;

namespace CacheAdapters.UsersCache
{
    public interface IUsersCacheAdapter
    {
        Task<bool> AddUserToCacheAsync(User user);
        Task<int> GetCurrentCachedUsersCountAsync();
        User GetUserFromCache(string userId);
        Task<User> GetUserFromCacheAsync(string userId);
        bool RemoveUserFromCache(string userId);
        Task<bool> RemoveUserFromCacheAsync(string userId);
    }
}