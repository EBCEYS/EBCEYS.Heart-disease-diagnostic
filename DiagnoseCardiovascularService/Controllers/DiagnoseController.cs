using DiagnoseCardiovascularService.Server;
using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;

namespace DiagnoseCardiovascularService.Controllers
{
    internal class DiagnoseController : RabbitMQControllerBase
    {
        private readonly ILogger<DiagnoseController> logger;
        private readonly IDiagnoseServer server;

        public DiagnoseController(ILogger<DiagnoseController> logger, IDiagnoseServer server)
        {
            this.logger = logger;
            this.server = server;
        }
        [RabbitMQMethod("PrepairedData")]
        public async Task Diagnose(PrepairedWetData data)
        {
            logger.LogDebug("Get Diagnose request: {@data}", data);
            await server.DiagnoseAsync(data);
        }
    }
}
