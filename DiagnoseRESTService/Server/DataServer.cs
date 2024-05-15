using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Client;
using HeartDiseasesDiagnosticExtentions.DataBaseExtensions;
using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.RabbitMQExtensions;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Microsoft.Extensions.Configuration;
using NLog;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UsersCache.DiagnoseCache;

namespace DiagnoseRestService.Server
{
    /// <summary>
    /// The data server.
    /// </summary>
    public class DataServer
    {
        /// <summary>
        /// The data server constructor.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        public DataServer(Logger logger, IConfiguration config, RabbitMQClient rpcClient, RabbitMQClient dbClient, IDiagnoseCacheAdapter diagnoseCache)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
            this.dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));
            this.diagnoseCache = diagnoseCache ?? throw new ArgumentNullException(nameof(diagnoseCache));

            eventsDict = new();

            rpcClient.StartAsync(CancellationToken.None).ConfigureAwait(false);
            dbClient.StartAsync(CancellationToken.None).ConfigureAwait(false);
        }

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly Logger logger;
        private readonly IConfiguration config;

        private readonly RabbitMQClient rpcClient;
        private readonly RabbitMQClient dbClient;
        private readonly IDiagnoseCacheAdapter diagnoseCache;
        private readonly ConcurrentDictionary<string, List<ManualResetEvent>> eventsDict;

        public async Task<RestActionResponse> RequestToCalcAsync<T>(string userRequestId, T data, DataSetTypes dataSetType, string userId) where T : class
        {
            ActionResponse result = await RabbitRequestToCalcAsync(data);
            DBWriteRequest requestToWrite = new()
            {
                DataSetType = dataSetType,
                Id = userRequestId,
                Request = data,
                Response = result,
                UserId = userId
            };
            await dbClient.SendRequestAsync<bool>(new()
            {
                Method = "WriteResult",
                Params = new[] { requestToWrite }
            }).ConfigureAwait(false);
            return new()
            {
                RequestId = userRequestId,
                Value = result.Value ?? null,
                Answer = result.Answer
            };
        }

        public async Task<ActionResponse> RabbitRequestToCalcAsync<T>(T data) where T : class
        {
            try
            {
                logger.Info("Request by rabbitMQ with data {@data}", data);
                BaseDiagnoseRequest request = new()
                {
                    Method = "Diagnose",
                    Params = new[] { data }
                };
                BaseDiagnoseResponse result = await rpcClient.SendRequestAsync<BaseDiagnoseResponse>(new()
                {
                    Method = "Diagnose",
                    Params = new[] { request }
                });
                if (result != null)
                {
                    return result.Result;
                }
                else
                {
                    throw new Exception("Empty request result!");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on posting Rpc rabbit request!", ex.Message);
                return new ActionResponse()
                {
                    Answer = Result.ERROR
                };
            }
        }

        public async Task<bool> RequestToWriteLineAsync(string requestId, string dataSetName, JsonDocument data, string userId)
        {
            DBWriteRequest request = new()
            {
                DataSetType = DataSetTypes.Unknown,
                Id = requestId,
                Request = data,
                UserId = userId
            };
            return await dbClient.SendRequestAsync<bool>(new()
            {
                Method = "WriteLine",
                Params = new[] { request }
            });
        }
        public async Task<bool> RequestToWriteLinesAsync(string requestId, string dataSetName, List<JsonDocument> data, string userId)
        {
            List<UnknownDataSet> datas = new();
            datas.AddRange(from item in data
                           select new UnknownDataSet(dataSetName, item));
            DBWriteRequest request = new()
            {
                DataSetType = DataSetTypes.Unknown,
                Id = requestId,
                Request = datas,
                UserId = userId
            };
            return await dbClient.SendRequestAsync<bool>(new()
            {
                Method = "WriteLines",
                Params = new[] { request }
            });
        }

        public async Task<List<DiagnoseResult>> GetDiagnoseResults(string sessionId, bool refresh)
        {
            return await Task.Run( async () =>
            {
                try
                {
                    if (refresh)
                    {
                        using ManualResetEvent ev = new(false);
                        if (!eventsDict.ContainsKey(sessionId))
                        {
                            eventsDict.TryAdd(sessionId, new());
                        }
                        eventsDict[sessionId].Add(ev);
                        ev.WaitOne(TimeSpan.FromSeconds(30));
                        eventsDict.TryRemove(sessionId, out _);
                    }
                    List<DiagnoseResult> data = await diagnoseCache.GetDiagnoseResultsIfExistsAsync(sessionId);
                    if (data == null)
                    {
                        return null;
                    }
                    await diagnoseCache.RemoveDiagnoseResultsDataAsync(sessionId);
                    return data;
                }
                catch(Exception ex)
                {
                    logger.Error(ex, "Error on getting diagnose results!");
                    throw ex;
                }
            });
        }

        public async Task AddDiagnoseResultToCache(DiagnoseResult result)
        {
            await diagnoseCache.AddDiagnoseResultsAsync(result.SessionId, result);
            if (eventsDict.TryGetValue(result.SessionId, out List<ManualResetEvent> events) && events != null)
            {
                events.ForEach(x => x.Set());
            }
            await SendResultsToDatabase(result);
        }

        private async Task<bool> SendResultsToDatabase(DiagnoseResult result)
        {
            return await dbClient.SendRequestAsync<bool>(new()
            {
                Method = "AsyncDiagnoseResult",
                Params = new[] { result }
            });
        }
    }
}
