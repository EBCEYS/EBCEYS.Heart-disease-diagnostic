using AlertStorageService.Server;
using DataBaseObjects.AlertDB;
using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;

namespace AlertStorageService.Controllers
{
    internal class AlertRabbitMQController : RabbitMQControllerBase
    {
        private readonly ILogger<AlertRabbitMQController> logger;
        private readonly DataLakeRepository repo;

        public AlertRabbitMQController(ILogger<AlertRabbitMQController> logger, DataLakeRepository repo)
        {
            this.logger = logger;
            this.repo = repo;
        }

        [RabbitMQMethod("UserNotFound")]
        public async Task UserNotFoundMethod(PrepairedWetData data)
        {
            logger.LogDebug("UserNotFoundMethod request: {@data}", data);
            await repo.UserNotFoundAlertAsync(data);
        }
        [RabbitMQMethod("ParseError")]
        public async Task ParseErrorMethod(PrepairedWetData data)
        {
            logger.LogDebug("ParseErrorMethod request: {@data}", data);
            await repo.ParseErrorAlertAsync(data);
        }
        [RabbitMQMethod("UnknownData")]
        public async Task UnknownDataMethod(PrepairedWetData data)
        {
            logger.LogDebug("UnknownDataMethod request: {@data}", data);
            await repo.UnknownDataAlertAsync(data);
        }
        [RabbitMQMethod("DiagnoseError")]
        public async Task DiagnoseErrorMethod(PrepairedWetData data)
        {
            logger.LogDebug("DiagnoseErrorMethod request: {@data}", data);
            await repo.DiagnoseErrorAlertAsync(data);
        }
    }
}
