using Redis.OM.Modeling;

namespace CacheAdapters.CacheModels
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "JwtCacheData" })]
    public class JwtCachedData
    {
        [RedisIdField][Indexed] public string RefreshToken { get; set; }
        [Indexed] public string UserId { get; set; }
        [Indexed] public string CurrentToken { get; set; }
        [Indexed(Sortable = true)] public DateTime DeletionDateTime { get; set; }
        public JwtCachedData(string refreshToken, string userId, string currentToken, TimeSpan timeToLive)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException($"\"{nameof(refreshToken)}\" не может быть пустым или содержать только пробел.", nameof(refreshToken));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"\"{nameof(userId)}\" не может быть пустым или содержать только пробел.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(currentToken))
            {
                throw new ArgumentException($"\"{nameof(currentToken)}\" не может быть пустым или содержать только пробел.", nameof(currentToken));
            }

            RefreshToken = refreshToken;
            UserId = userId;
            CurrentToken = currentToken;
            DeletionDateTime = DateTime.UtcNow.Add(timeToLive);
        }
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public JwtCachedData()
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        {
                
        }
    }
}
