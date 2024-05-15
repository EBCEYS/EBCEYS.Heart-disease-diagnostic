using DataBaseObjects.DiagnoseDB;
using DBAdapterService;
using DBAdapterService.RabbitMQControllers;
using HeartDiseasesDiagnosticExtentions.DataBaseExtensions;
using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NLog;
using System.Text.Json;

namespace DBAdapterServiceTests
{
    [TestClass]
    public class DBAdapterService_DBRabbitMQController_Tests
    {
        private readonly DBWriteRequest baseDiagnoseRequest = new()
        {
            Id = "123",
            DataSetType = DataSetTypes.Unknown,
            Request = JsonSerializer.Serialize(new { a = 1 })!,
            Response = new()
            {
                Answer = Result.OK,
                Value = 1
            },
            UserId = "userId"
        };

        private readonly DBWriteRequest baseDiagnoseRequestArray = new()
        {
            Id = "123",
            DataSetType = DataSetTypes.Unknown,
            Request = JsonSerializer.Serialize(new object[]
            {
                new {a = 1},
                new {a = 2}
            })!,
            Response = new()
            {
                Answer = Result.OK,
                Value = 1
            },
            UserId = "userId"
        };

        private readonly HashSet<DiagnoseData> diagnoseDataStorage = new();
        private readonly HashSet<DataToStorage> dataToStorageStorage = new();

        DBRabbitMQController? controller;

        [TestInitialize]
        public void Initialize()
        {
            Mock<DBRepository> mock = new(LogManager.CreateNullLogger(), null);
            mock.Setup(x => x.WriteResultAsync(baseDiagnoseRequest)).ReturnsAsync(() =>
            {
                DiagnoseData data = new()
                {
                    Id = baseDiagnoseRequest.Id,
                    Params = JsonSerializer.Deserialize<JsonDocument>(baseDiagnoseRequest.Request!.ToString()!),
                    Result = baseDiagnoseRequest.Response.Answer,
                    ResultValue = baseDiagnoseRequest.Response.Value ?? -1.0
                };
                return diagnoseDataStorage.Add(data);
            });
            mock.Setup(x => x.WriteLineAsync(baseDiagnoseRequest)).ReturnsAsync(() =>
            {
                DataToStorage data = new()
                {
                    Id = baseDiagnoseRequest.Id,
                    Data = JsonSerializer.Deserialize<JsonDocument>(baseDiagnoseRequest.Request.ToString()!),
                    UserKey = baseDiagnoseRequest.UserId,
                };
                return dataToStorageStorage.Add(data);
            });
            mock.Setup(x => x.WriteLinesAsync(baseDiagnoseRequestArray)).ReturnsAsync(() =>
            {
                JsonDocument[] datas = JsonSerializer.Deserialize<JsonDocument[]>(baseDiagnoseRequestArray.Request.ToString()!)!;
                DataToStorage data = new()
                {
                    Id = baseDiagnoseRequestArray.Id,
                    Data = JsonSerializer.Deserialize<JsonDocument>(baseDiagnoseRequestArray.Request.ToString()!),
                    UserKey = baseDiagnoseRequestArray.UserId,
                };
                bool res = true;
                for (int i = 0; i < datas.Length; i++)
                {
                    res &= dataToStorageStorage.Add(new()
                    {
                        Id = $"{i}_{baseDiagnoseRequestArray.Id}",
                        Data = datas[i],
                        UserKey = baseDiagnoseRequestArray.UserId
                    });
                }
                return res;
            });

            controller = new(new Logger<DBRabbitMQController>(new NullLoggerFactory()), mock.Object);
        }
        [TestMethod]
        public async Task WriteResultAsync_Test()
        {
            diagnoseDataStorage.Clear();
            dataToStorageStorage.Clear();

            await controller!.WriteResultAsync(baseDiagnoseRequest);

            Assert.IsTrue(diagnoseDataStorage.Any());
        }
        [TestMethod]
        public async Task WriteLineAsync_Test()
        {
            diagnoseDataStorage.Clear();
            dataToStorageStorage.Clear();

            await controller!.WriteLineAsync(baseDiagnoseRequest);

            Assert.IsTrue(dataToStorageStorage.Any());
        }
        [TestMethod]
        public async Task WriteLinesAsync_Test()
        {
            diagnoseDataStorage.Clear();
            dataToStorageStorage.Clear();

            await controller!.WriteLinesAsync(baseDiagnoseRequestArray);

            Assert.IsTrue(dataToStorageStorage.Any());
        }
    }
}