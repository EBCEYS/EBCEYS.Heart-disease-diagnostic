using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using System.Collections.Concurrent;

namespace DefineDataService.Controllers
{
    internal class DataToDiagnoseController : RabbitMQControllerBase
    {
        private readonly ILogger<DataToDiagnoseController> logger;
        private readonly ConcurrentQueue<PrepairedWetData> dataCollection;

        public DataToDiagnoseController(ILogger<DataToDiagnoseController> logger, ConcurrentQueue<PrepairedWetData> dataCollection)
        {
            this.logger = logger;
            this.dataCollection = dataCollection;
        }

        [RabbitMQMethod("WetData")]
        public Task ProcessWetData(PrepairedWetData wetData)
        {
            logger.LogDebug("Get ProcessWetData request: {@wetData}", wetData);
            dataCollection.Enqueue(wetData);
            return Task.CompletedTask;
        }
    }
}
