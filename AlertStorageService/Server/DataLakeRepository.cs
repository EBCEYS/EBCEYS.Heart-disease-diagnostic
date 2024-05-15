using DataBaseObjects.HDDSDBContext;
using DiagnoseDataObjects;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace AlertStorageService.Server
{
    internal class DataLakeRepository
    {
        private readonly ILogger<DataLakeRepository> logger;
        private readonly DiagnoseDBContext db;

        public DataLakeRepository(ILogger<DataLakeRepository> logger, DiagnoseDBContext db)
        {
            this.logger = logger;
            this.db = db;
        }
        public async Task<bool> UserNotFoundAlertAsync(PrepairedWetData data)
        {
            try
            {
                await db.Alerts.AddAsync(new()
                {
                    Data = JsonConvert.SerializeObject(data),
                    Level = DataBaseObjects.AlertDB.AlertLevel.Warning,
                    Type = DataBaseObjects.AlertDB.AlertType.UserNotFound,
                    Id = CreateHash(data),
                });
                return (await db.SaveChangesAsync()) > 0;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on adding UserNotFoundAlertAsync!");
                return false;
            }
        }
        public async Task<bool> ParseErrorAlertAsync(PrepairedWetData data)
        {
            try
            {
                await db.Alerts.AddAsync(new()
                {
                    Data = JsonConvert.SerializeObject(data),
                    Level = DataBaseObjects.AlertDB.AlertLevel.Warning,
                    Type = DataBaseObjects.AlertDB.AlertType.ParsingError,
                    Id = CreateHash(data),
                    UserId = data.UserId
                });
                return (await db.SaveChangesAsync()) > 0;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on adding ParseErrorAlertAsync!");
                return false;
            }
        }
        public async Task<bool> UnknownDataAlertAsync(PrepairedWetData data)
        {
            try
            {
                await db.Alerts.AddAsync(new()
                {
                    Data = JsonConvert.SerializeObject(data),
                    Level = DataBaseObjects.AlertDB.AlertLevel.Warning,
                    Type = DataBaseObjects.AlertDB.AlertType.UnknownDataType,
                    Id = CreateHash(data),
                    UserId = data.UserId
                });
                return (await db.SaveChangesAsync()) > 0;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on adding UnknownDataAlertAsync!");
                return false;
            }
        }
        public async Task<bool> DiagnoseErrorAlertAsync(PrepairedWetData data)
        {
            try
            {
                await db.Alerts.AddAsync(new()
                {
                    Data = JsonConvert.SerializeObject(data),
                    Level = DataBaseObjects.AlertDB.AlertLevel.Error,
                    Type = DataBaseObjects.AlertDB.AlertType.DiagnoseError,
                    Id = CreateHash(data),
                    UserId = data.UserId
                });
                return (await db.SaveChangesAsync()) > 0;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error on adding DiagnoseErrorAlertAsync");
                return false;
            }
        }
        private static string CreateHash(PrepairedWetData data)
        {
            string dataToHash = $"{data.UserId ?? "notfound"}_{data.InputData!.SessionId}_{data.InputData.DataType}_{Guid.NewGuid()}";
            byte[] hashedData = SHA256.HashData(Encoding.UTF8.GetBytes(dataToHash));
            return Convert.ToBase64String(hashedData);
        }
    }
}
