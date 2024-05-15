using DataBaseObjects.UsersDB;
using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Client;
using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.RabbitMQExtensions;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace DefineDataService.Server
{
    internal class DefineDataProcessor : IHostedService
    {
        private readonly ILogger<DefineDataProcessor> logger;
        private readonly RabbitMQClient usersClient;
        private readonly RabbitMQClient clevelandClient;
        private readonly RabbitMQClient heartFailureClient;
        private readonly RabbitMQClient maleCardiovascularClient;
        private readonly RabbitMQClient allertClient;

        private readonly ConcurrentQueue<PrepairedWetData> dataCollection;

        private readonly Thread process;

        public DefineDataProcessor(ILogger<DefineDataProcessor> logger, ILogger<RabbitMQClient> mqLogger, IConfiguration config, ConcurrentQueue<PrepairedWetData> dataCollection)
        {
            this.logger = logger;

            this.dataCollection = dataCollection;

            usersClient = new(mqLogger, config.GetRabbitMQConfiguration("UsersRabbitMQClient"), TimeSpan.FromSeconds(5));
            clevelandClient = new(mqLogger, config.GetRabbitMQConfiguration("ClevelandMQClient"));
            heartFailureClient = new(mqLogger, config.GetRabbitMQConfiguration("HeartFailureMQClient"));
            maleCardiovascularClient = new(mqLogger, config.GetRabbitMQConfiguration("MaleCardiovascularMQClient"));
            allertClient = new(mqLogger, config.GetRabbitMQConfiguration("AllertMQClient"));

            process = new(ProcessData!);
        }

        public void ProcessData(object state)
        {
            CancellationToken token = (CancellationToken)state;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    while (dataCollection.TryDequeue(out PrepairedWetData? data) && data != null)
                    {
                        try
                        {
                            await ProcessDataAction(data);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error on processing data: {@data}", data);
                        }
                    }
                    await Task.Delay(10);
                }
            }, token);
        }

        public virtual async Task ProcessDataAction(PrepairedWetData data)
        {
            User? user = await usersClient.SendRequestAsync<User?>(new()
            {
                Method = "GetUser",
                Params = new [] { data.UserId! }
            });
            if (user == null)
            {
                logger.LogWarning("User with id: {id} is not found!", data.UserId);
                await allertClient.SendMessageAsync(new()
                {
                    Method = "UserNotFound",
                    Params = new [] { data }
                });
            }
            switch(data.InputData!.DataType)
            {
                case DataSetTypes.CardiovascularDiseaseDataSet:
                    CardiovascularDiseaseDataSet? card = data.InputData!.DataToDiagnose!.ToObject<CardiovascularDiseaseDataSet>();
                    if (card == null || !card.CheckAttributes(out _))
                    {
                        logger.LogWarning("Can not parse data to {type}: {@data}", typeof(CardiovascularDiseaseDataSet), data.InputData.DataToDiagnose);
                        await allertClient.SendMessageAsync(new()
                        {
                            Method = "ParseError",
                            Params = new[] { data }
                        });
                        return;
                    }
                    data.InputData!.DataToDiagnose = JObject.FromObject(card);
                    await clevelandClient.SendMessageAsync(new()
                    {
                        Method = "PrepairedData",
                        Params = new [] { data }
                    });
                    break;
                case DataSetTypes.HeartFailurePredictionDataSet:
                    HeartFailurePredictionDataSet? heart = data.InputData!.DataToDiagnose!.ToObject<HeartFailurePredictionDataSet>();
                    if (heart == null || !heart.CheckAttributes(out _))
                    {
                        logger.LogWarning("Can not parse data to {type}: {@data}", typeof(HeartFailurePredictionDataSet), data.InputData.DataToDiagnose);
                        await allertClient.SendMessageAsync(new()
                        {
                            Method = "ParseError",
                            Params = new[] { data }
                        });
                        return;
                    }
                    data.InputData!.DataToDiagnose = JObject.FromObject(heart);
                    await heartFailureClient.SendMessageAsync(new()
                    {
                        Method = "PrepairedData",
                        Params = new [] { data }
                    });
                    break;
                case DataSetTypes.MaleCardiovascularDiseaseDataSet:
                    MaleCardiovascularDiseaseDataSet? male = data.InputData!.DataToDiagnose!.ToObject<MaleCardiovascularDiseaseDataSet>();
                    if (male == null || !male.CheckAttributes(out _))
                    {
                        logger.LogWarning("Can not parse data to {type}: {@data}", typeof(MaleCardiovascularDiseaseDataSet), data.InputData.DataToDiagnose);
                        await allertClient.SendMessageAsync(new()
                        {
                            Method = "ParseError",
                            Params = new[] { data }
                        });
                        return;
                    }
                    data.InputData!.DataToDiagnose = JObject.FromObject(male);
                    await maleCardiovascularClient.SendMessageAsync(new()
                    {
                        Method = "PrepairedData",
                        Params = new [] { data }
                    });
                    break;
                default:
                    await allertClient.SendMessageAsync(new()
                    {
                        Method = "UnknownData",
                        Params = new [] { data }
                    });
                    break;
            }
        }

        public Task AddWetDataToProcess(PrepairedWetData wetData)
        {
            dataCollection.Enqueue(wetData);
            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                process.Start(cancellationToken);
            }
            catch
            {

            }
            await usersClient.StartAsync(cancellationToken);
            await clevelandClient.StartAsync(cancellationToken);
            await heartFailureClient.StartAsync(cancellationToken);
            await maleCardiovascularClient.StartAsync(cancellationToken);
            await allertClient.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await usersClient.StopAsync(cancellationToken);
            await clevelandClient.StopAsync(cancellationToken);
            await heartFailureClient.StopAsync(cancellationToken);
            await maleCardiovascularClient.StopAsync(cancellationToken);
            await allertClient.StopAsync(cancellationToken);
        }  
    }      
}          
