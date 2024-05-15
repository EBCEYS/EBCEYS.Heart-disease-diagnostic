using CacheAdapters.CacheModels;
using DataBaseObjects.UsersDB;
using Microsoft.EntityFrameworkCore;
using Redis.OM;
using Redis.OM.Searching;
using Redis.OM.Searching.Query;
using StackExchange.Redis;
using UsersCache;

namespace CacheAdapters.UsersCache
{
    public class UsersCacheAdapter : IUsersCacheAdapter
    {
        private readonly RedisCollection<CacheUser> users;

        public UsersCacheAdapter(CacheAdapterSettings settings)
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
            RedisConnectionProvider provider = new(config);//,password={settings.Password!}");
            provider.Connection.CreateIndex(typeof(CacheUser));
            users = (RedisCollection<CacheUser>)provider.RedisCollection<CacheUser>();
        }

        public async Task<bool> AddUserToCacheAsync(User user)
        {
            if (await users.FindByIdAsync(user.Id!) == null)
            {
                string key = await users.InsertAsync(new CacheUser(user, TimeSpan.FromSeconds(180)));//TODO: добавить передачу времени жизни токена
                return !string.IsNullOrEmpty(key);
            }
            await users.UpdateAsync(new CacheUser(user, TimeSpan.FromSeconds(180)));
            return true;
        }

        public async Task<User> GetUserFromCacheAsync(string userId)
        {
            CacheUser? user = await users.FindByIdAsync(userId);
            return user == null ? throw new RedisException("User is not found!") : user.ToDBUser();
        }
        public User GetUserFromCache(string userId)
        {
            CacheUser? user = users.FindById(userId);
            return user == null ? throw new RedisException("User is not found!") : user.ToDBUser();
        }
        public bool RemoveUserFromCache(string userId)
        {
            CacheUser? user = users.FindById(userId);
            if (user == null)
            {
                return false;
            }
            users.Delete(user);
            return true;
        }

        public async Task<bool> RemoveUserFromCacheAsync(string userId)
        {
            CacheUser? user = await users.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            await users.DeleteAsync(user);
            return true;
        }

        public async Task<List<CacheUser>?> GetUsersByTimeToLiveAsync(int countToTake = 100)
        {
            IList<CacheUser>? res = await users.OrderBy(u => u.DeletionDateTime).Take(countToTake).ToListAsync();
            if (res == null)
            {
                return null;
            }
            return res.Where(u => DateTimeOffset.UtcNow > u.DeletionDateTime).ToList();
        }

        public async Task<bool> RemoveUsersAsync(params string[] userIds)
        {
            IList<CacheUser?>? usrs = (await users.FindByIdsAsync(userIds))?.Select(x => x.Value)?.ToList();
            if (usrs == null) return false;
            await users.DeleteAsync(usrs!);
            return true;
        }

        public async Task<int> GetCurrentCachedUsersCountAsync() => await users.CountAsync();
    }
}