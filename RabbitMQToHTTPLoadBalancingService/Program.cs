using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using HeartDiseasesDiagnosticExtentions.JsonExtensions;
using HeartDiseasesDiagnosticExtentions.RabbitMQExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using RabbitMQToHTTPLoadBalancingService.Controllers;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitMQToHTTPLoadBalancingService
{
    public class Program
    {
        public static string BasePath { get; set; }
        private static RabbitMQConfiguration RabbitMQConfig { get; set; }
        public static JsonSerializerOptions SerializerOptions { get; } = new()
        {
            Converters = { new JsonStringEnumConverter(), new JsonStringConverter() },
            WriteIndented = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        static Logger logger;
        public static void Main(string[] args)
        {
            BasePath = GetBasePath();
            string pth = Path.Combine(BasePath, "nlog.config");
            logger = LogManager.Setup().LoadConfigurationFromFile(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = BasePath;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot config = builder.Build();

            RabbitMQConfig = config.GetRabbitMQConfiguration("RabbitMQConsumer");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddRabbitMQController<RabbitMQController>();
                    services.AddRabbitMQMappedServer(RabbitMQConfig);

                    services.AddSingleton(logger);
                    services.AddSingleton<IPStorage>();
                }).ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.SetBasePath(GetBasePath());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                }).UseNLog();
        }

        private static string GetBasePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
