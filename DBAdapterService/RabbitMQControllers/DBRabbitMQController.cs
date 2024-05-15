using DataBaseObjects.DiagnoseDB;
using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.Controllers;
using HeartDiseasesDiagnosticExtentions.DataBaseExtensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBAdapterService.RabbitMQControllers
{
    public class DBRabbitMQController : RabbitMQControllerBase
    {
        private readonly ILogger<DBRabbitMQController> logger;
        private readonly DBRepository repository;

        public DBRabbitMQController(ILogger<DBRabbitMQController> logger, DBRepository repository)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.repository = repository ?? throw new System.ArgumentNullException(nameof(repository));
        }

        [RabbitMQMethod("WriteResult")]
        public async Task<bool> WriteResultAsync(DBWriteRequest request)
        {
            bool result = await repository.WriteResultAsync(request);
            logger.LogInformation("Write result request result is {result}", result);
            return result;
        }
        [RabbitMQMethod("WriteLine")]
        public async Task<bool> WriteLineAsync(DBWriteRequest request)
        {
            bool result = await repository.WriteLineAsync(request);
            logger.LogInformation("Write line request result is {result}", result);
            return result;
        }
        [RabbitMQMethod("WriteLines")]
        public async Task<bool> WriteLinesAsync(DBWriteRequest request)
        {
            bool result = await repository.WriteLinesAsync(request);
            logger.LogInformation("Write lines request result is {result}", result);
            return result;
        }
        [RabbitMQMethod("AsyncDiagnoseResult")]
        public async Task<bool> WriteAsyncDiagnoseResults(DiagnoseResult result)
        {
            bool res = await repository.WriteAsynchronousDiagnoseResults(result);
            logger.LogInformation("Write async diagnose results request result is: {@result}", res);
            return res;
        }
    }
}
