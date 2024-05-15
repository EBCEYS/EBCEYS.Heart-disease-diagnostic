using CacheAdapters.JwtCache;
using JwtCacheChecker.DataWorker;
using NLog;
using NLog.Web;
using UsersCache;

namespace JwtCacheChecker
{
    public class Program
    {
        public static string BasePath => GetBasePath();
        public static void Main(string[] args)
        {
            string pth = Path.Combine(BasePath, "nlog.config");
            Logger logger = LogManager.Setup().LoadConfigurationFromFile(pth).GetCurrentClassLogger();
            //Logger logger = NLogBuilder.ConfigureNLog(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = BasePath;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot config = builder.Build();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.SetBasePath(GetBasePath());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IJwtCacheAdapter>(sr =>
                    {
                        return new JwtCacheAdapter(config.GetSection("JwtCacheSettings").Get<CacheAdapterSettings>()!);
                    });
                    services.AddHostedService(sr =>
                    {
                        return new RedisJwtCacheWorker(sr.GetService<ILogger<RedisJwtCacheWorker>>()!, sr.GetService<IJwtCacheAdapter>()!, sr.GetService<IConfiguration>()!);
                    });
                })
                .ConfigureLogging(log =>
                {
                    log.ClearProviders();
                    log.AddNLog(logger.Factory.Configuration);
                })
                .Build();

            host.Run();
        }

        private static string GetBasePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}