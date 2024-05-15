using DataBaseObjects.AlertDB;
using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Client;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace DiagnoseCardiovascularService.Server
{
    internal class DiagnoseServer : IDiagnoseServer
    {
        private readonly ILogger<DiagnoseServer> logger;
        private readonly RabbitMQClient client;
        private readonly ConcurrentQueue<PrepairedWetData> alertsQueue;

        public DiagnoseServer(ILogger<DiagnoseServer> logger, RabbitMQClient client, ConcurrentQueue<PrepairedWetData> alertsQueue)
        {
            this.logger = logger;
            this.client = client;
            this.alertsQueue = alertsQueue;
        }

        public async Task DiagnoseAsync(PrepairedWetData data)
        {
            try
            {
                Random rnd = new();
                DiagnoseResult result = new()
                {
                    ResultValue = rnd.NextDouble(),
                    UserId = data.UserId,
                    SessionId = data.InputData!.SessionId,
                    DataType = HeartDiseasesDiagnosticExtentions.DataSetsClasses.DataSetTypes.CardiovascularDiseaseDataSet,
                    DiagnoseData = data.InputData.DataToDiagnose,
                    Result = HeartDiseasesDiagnosticExtentions.ResponseExtensions.Result.OK
                };
                logger.LogDebug("Diagnose result is: {@result}", result);
                await client.SendMessageAsync(new()
                {
                    Method = "DiagnoseResult",
                    Params = new[] { result }
                });
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error while diagnosing!");
                alertsQueue.Enqueue(data);
            }
        }
    }
}
