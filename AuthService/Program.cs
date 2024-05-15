using CacheAdapters.JwtCache;
using CacheAdapters.UsersCache;
using NLog;
using NLog.Web;
using UsersCache;

namespace AuthService
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The port.
        /// </summary>
        public static int Port { get; set; } = 5000;
        /// <summary>
        /// The base path.
        /// </summary>
        public static string BasePath { get; set; }
        static Logger logger;

        /// <summary>
        /// Gets the secret key bytes.
        /// </summary>
        public static byte[] SecretKey => secretKey;
        private static byte[] secretKey;
        /// <summary>
        /// The cache adapter.
        /// </summary>
        public static IUsersCacheAdapter CacheAdapter { get; private set; }
        /// <summary>
        /// The jwt cache adapter.
        /// </summary>
        public static IJwtCacheAdapter JwtCacheAdapter { get; private set; }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            BasePath = GetBasePath();
            string pth = Path.Combine(BasePath, "nlog.config");
            logger = LogManager.Setup().LoadConfigurationFromFile(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = BasePath;

            SetSecretKey(Path.Combine(BasePath, "secret.key"));

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();

            CacheAdapter = new UsersCacheAdapter(configuration.GetSection("CacheSettings").Get<CacheAdapterSettings>());
            JwtCacheAdapter = new JwtCacheAdapter(configuration.GetSection("JwtCacheSettings").Get<CacheAdapterSettings>());

            Port = int.Parse(configuration.GetSection("Port").Value);

            logger.Info("Starting app:");

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://0.0.0.0:{Port}");
                })
                .ConfigureAppConfiguration((builderContext, config) =>
                { 
                    config.SetBasePath(BasePath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddNLog(logger.Factory.Configuration);
                    logging.AddNLogWeb();
                }).UseNLog();
        }

        private static void SetSecretKey(string secretPath)
        {
            if (File.Exists(secretPath))
            {
                secretKey = File.ReadAllBytes(secretPath);
                return;
            }
            throw new FileNotFoundException(secretPath);
        }

        private static string GetBasePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}