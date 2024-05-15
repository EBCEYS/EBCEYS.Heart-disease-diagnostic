namespace CacheContracts
{
    /// <summary>
    /// The base cache contract response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseCacheResponse<T>
    {
        /// <summary>
        /// The cache response result.
        /// </summary>
        public CacheContractResult Result { get; set; }
        /// <summary>
        /// The cache response object.
        /// </summary>
        public T? Object { get; set; }
    }
}