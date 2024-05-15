using NLog;
using NLog.Web;

namespace HolterAnalyzeRest
{
    /// <summary>
    /// The program.
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
        /// The main method.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            BasePath = GetBasePath();
            string pth = Path.Combine(BasePath, "nlog.config");
            logger = NLogBuilder.ConfigureNLog(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = BasePath;


            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();

            Port = int.Parse(configuration.GetSection("Port").Value);

            logger.Info("Starting app with port {port}:", Port);

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The host builder.</returns>
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
                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                });
        }

        private static string GetBasePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
