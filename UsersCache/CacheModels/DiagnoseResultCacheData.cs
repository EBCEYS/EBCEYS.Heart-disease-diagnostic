using Redis.OM.Modeling;

namespace CacheAdapters.CacheModels
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "DiagnoseResults" })]
    public class DiagnoseResultCacheData
    {
        [RedisIdField][Indexed] public string Id { get; set; }
        [Indexed] public List<CacheDiagnoseResult> DiagnoseResults { get; set; } = new();
        [Indexed(Sortable = true)] public DateTimeOffset DeletionDateTime { get; set; }
        public DiagnoseResultCacheData(string id, TimeSpan timeToLive)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"\"{nameof(id)}\" не может быть пустым или содержать только пробел.", nameof(id));
            }
            Id = id;
            DeletionDateTime = DateTimeOffset.UtcNow + timeToLive;
        }
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public DiagnoseResultCacheData()
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        {
                
        }
    }
}
