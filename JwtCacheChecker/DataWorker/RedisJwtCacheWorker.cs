using CacheAdapters.JwtCache;

namespace JwtCacheChecker.DataWorker
{
    internal class RedisJwtCacheWorker : IHostedService
    {
        private readonly ILogger<RedisJwtCacheWorker> logger;
        private readonly IConfiguration config;
        private readonly IJwtCacheAdapter jwtCacheAdapter;

        private readonly TimeSpan delay;

        public RedisJwtCacheWorker(ILogger<RedisJwtCacheWorker> logger, IJwtCacheAdapter jwtCache, IConfiguration config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.jwtCacheAdapter = jwtCache ?? throw new ArgumentNullException(nameof(jwtCache));
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            delay = TimeSpan.FromSeconds(this.config.GetValue<double?>("DelayBeforeIterations") ?? 5.0);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Start cache process thread!");
            Task.Run(async () => await JwtCacheProcessAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        private async Task JwtCacheProcessAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    List<CacheAdapters.CacheModels.JwtCachedData>? dataToRemove = await jwtCacheAdapter.GetTimeoutedJwtDataAsync();
                    if (dataToRemove != null && dataToRemove.Any())
                    {
                        logger.LogDebug("Get data to remove: {@data}", dataToRemove.Select(x => x.RefreshToken));
                        bool removeRes = await jwtCacheAdapter.RemoveJwtDataAsync(dataToRemove.ToArray());
                        logger.LogDebug("Remove result is: {res}", removeRes);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on processing jwt cache!");
                }
                await Task.Delay(delay, cancellationToken);
            }
        }

        private async Task UsersCacheProcessAsync(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {

            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
