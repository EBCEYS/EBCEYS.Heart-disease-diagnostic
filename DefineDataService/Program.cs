using DefineDataService.Controllers;
using DefineDataService.Server;
using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using HeartDiseasesDiagnosticExtentions.RabbitMQExtensions;
using NLog;
using NLog.Web;
using System.Collections.Concurrent;

namespace DefineDataService
{
    public class Program
    {
        public static string BasePath => GetBasePath();
        private static RabbitMQConfiguration? mqConfig;
        private static ConcurrentQueue<PrepairedWetData> dataCollection = new();
        public static void Main(string[] args)
        {
            string pth = Path.Combine(BasePath, "nlog.config");
            Logger logger = LogManager.Setup().LoadConfigurationFromFile(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = BasePath;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot config = builder.Build();

            mqConfig = config.GetRabbitMQConfiguration("RabbitMQMappedService");

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.SetBasePath(GetBasePath());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(dataCollection);
                    services.AddHostedService<DefineDataProcessor>();
                    services.AddSingleton(sr =>
                    {
                        return sr.GetService<DefineDataProcessor>()!;
                    });
                    services.AddRabbitMQController<DataToDiagnoseController>();
                    services.AddRabbitMQMappedServer(mqConfig!);
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