using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using System.Collections.Concurrent;

namespace DiagnoseCardiovascularService.AlertsProcessor
{
    internal class AlertsMQProcessor : IHostedService
    {
        private readonly ILogger<AlertsMQProcessor> logger;
        private readonly ConcurrentQueue<PrepairedWetData> alertsQueue;
        private readonly RabbitMQClient alertsClient;

        private readonly Thread alertsSenderThread;

        public AlertsMQProcessor(ILogger<AlertsMQProcessor> logger, ILogger<RabbitMQClient> mqLogger, ConcurrentQueue<PrepairedWetData> alertsQueue, RabbitMQConfiguration alertsConfig)
        {
            this.logger = logger;
            this.alertsQueue = alertsQueue;
            alertsClient = new(mqLogger, alertsConfig);
            alertsSenderThread = new(ProcessAlerts!);
        }

        private void ProcessAlerts(object state)
        {
            CancellationToken token = (CancellationToken)state;
            while(!token.IsCancellationRequested)
            {
                while(alertsQueue.TryDequeue(out PrepairedWetData? alert) && alert != null)
                {
                    Task.Run(async () => await alertsClient.SendMessageAsync(new()
                    {
                        Method = "DiagnoseError",
                        Params = new[] { alert }
                    }), token);
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                alertsSenderThread.Start(cancellationToken);
            }
            catch { }
            await alertsClient.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await alertsClient.StopAsync(cancellationToken);
        }
    }
}
