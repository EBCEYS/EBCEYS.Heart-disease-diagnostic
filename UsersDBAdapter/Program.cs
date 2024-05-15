using DataBaseObjects.HDDSDBContext;
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
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using UsersDBAdapter.DataBaseRepository;
using UsersDBAdapter.RabbitMQControllers;

namespace UsersDBAdapter
{
    public class Program
    {
        public static JsonSerializerOptions SerializerOptions { get; } = new()
        {
            Converters = { new JsonStringEnumConverter(), new JsonStringConverter() },
            WriteIndented = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        public static string BasePath { get; set; }
        private static Logger logger;
        public static RabbitMQConfiguration RabbitMQConfig { get; private set; }

        public static void Main(string[] args)
        {
            BasePath = AppDomain.CurrentDomain.BaseDirectory;
            string pth = Path.Combine(BasePath, "nlog.config");
            logger = LogManager.Setup().LoadConfigurationFromFile(pth).GetCurrentClassLogger();
            LogManager.Configuration.Variables["logDir"] = BasePath;

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(BasePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot config = builder.Build();

            RabbitMQConfig = config.GetRabbitMQConfiguration("RabbitMQConfig");

            CreateHostBuilder(args
                ).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(log =>
                {
                    log.ClearProviders();
                    log.AddNLog(logger.Factory.Configuration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<DiagnoseDBContext>();
                    services.AddScoped<IUsersDBRepository, UsersDBRepository>();

                    services.AddRabbitMQController<RabbitMQUsersDBAdapterController>();
                    services.AddRabbitMQMappedServer(RabbitMQConfig);
                }).ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.SetBasePath(BasePath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                }).UseNLog();
        }
    }
}
