using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DiagnoseRestService
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            string pth = Path.Combine(GetBasePath(), "nlog.config");
            logger = NLogBuilder.ConfigureNLog(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = GetBasePath();

            SetSecretKey(Path.Combine(GetBasePath(), "secret.key"));

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(GetBasePath())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();

            CreateHostBuilder(args).Build().Run();
        }

        private static Logger logger;
        public static byte[] SecretKey => secretKey;
        private static byte[] secretKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureAppConfiguration((builderContext, config) => 
                {
                    config.SetBasePath(GetBasePath());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                }).ConfigureLogging(logging =>
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
