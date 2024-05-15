using CacheAdapters.UsersCache;
using NLog;
using NLog.Web;
using UsersCache;

namespace DiagnoseReceiver
{
    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        private static Logger? logger;

        /// <summary>
        /// Gets service base path.
        /// </summary>
        public static string BasePath => GetBasePath();
        private static int? Port { get; set; }
        /// <summary>
        /// The secret key.
        /// </summary>
        public static byte[]? SecretKey { get; set; }
        /// <summary>
        /// The cache adapter.
        /// </summary>
        public static IUsersCacheAdapter? CacheAdapter { get; private set; }
        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            string pth = Path.Combine(BasePath, "nlog.config");
            logger = LogManager.Setup().LoadConfigurationFromFile(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = BasePath;

            SetSecretKey(Path.Combine(BasePath, "secret.key"));

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();

            CacheAdapter = new UsersCacheAdapter(configuration.GetSection("CacheSettings").Get<CacheAdapterSettings>()!);

            Port = int.Parse(configuration.GetSection("Port").Value ?? "-1");

            logger.Info("Starting app:");

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    if (Port != null && Port != -1)
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
                    logging.AddNLog(logger!.Factory.Configuration);
                    logging.AddNLogWeb();
                }).UseNLog();
        }

        private static void SetSecretKey(string secretPath)
        {
            if (File.Exists(secretPath))
            {
                SecretKey = File.ReadAllBytes(secretPath);
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