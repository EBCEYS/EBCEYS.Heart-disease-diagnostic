using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RabbitMQToHTTPLoadBalancingService.Controllers
{
    internal class RabbitMQController : RabbitMQControllerBase
    {
        private readonly ILogger<RabbitMQController> logger;
        private readonly IPStorage ipStorage;

        public RabbitMQController(ILogger<RabbitMQController> logger, IPStorage ipStorage)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ipStorage = ipStorage ?? throw new ArgumentNullException(nameof(ipStorage));
        }
        [RabbitMQMethod("Diagnose")]
        public async Task<BaseDiagnoseResponse> Diagnose(BaseDiagnoseRequest request)
        {
            logger.LogDebug("Get diagnose request: {@request}", request);
            string result = await ipStorage.RequestAndResponseAsync(request, 5.0);
            logger.LogDebug("Diagnose result is: {result}", result);

            BaseDiagnoseResponse response;

            if (string.IsNullOrWhiteSpace(result))
            {
                response = new()
                {
                    Result = new()
                    {
                        Answer = Result.ERROR,
                        Value = null
                    }
                };
            }
            else
            {
                response = ipStorage.ToObject<BaseDiagnoseResponse>(result) ?? new()
                {
                    Result = new()
                    {
                        Answer = Result.ERROR,
                        Value = null
                    }
                };
            }
            logger.LogDebug("Response is {@response}", response);
            return response;
        }
    }
}
