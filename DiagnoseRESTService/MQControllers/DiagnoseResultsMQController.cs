using DiagnoseDataObjects;
using DiagnoseRestService.Server;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiagnoseRESTService.MQControllers
{
    public class DiagnoseResultsMQController : RabbitMQControllerBase
    {
        private readonly ILogger<DiagnoseResultsMQController> logger;
        private readonly DataServer server;

        public DiagnoseResultsMQController(ILogger<DiagnoseResultsMQController> logger, DataServer server)
        {
            this.logger = logger;
            this.server = server;
        }
        [RabbitMQMethod("DiagnoseResult")]
        public async Task DiagnoseResults(DiagnoseResult result)
        {
            logger.LogInformation("Get diagnose results request: {@result}", result);
            await server.AddDiagnoseResultToCache(result);
        }
    }
}
