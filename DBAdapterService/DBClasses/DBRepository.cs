using DataBaseObjects.DiagnoseDB;
using DataBaseObjects.HDDSDBContext;
using DataBaseObjects.UsersDB;
using DiagnoseDataObjects;
using HeartDiseasesDiagnosticExtentions.DataBaseExtensions;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DBAdapterService
{
    public class DBRepository
    {

        private readonly Logger logger;
        private readonly DiagnoseDBContext db;

        public DBRepository(Logger logger, DiagnoseDBContext db)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.db = db;
        }

        public virtual async Task<bool> WriteResultAsync(DBWriteRequest request)
        {
            logger.Info("Write result request: {@request}", request);
            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive);
            if (user == null)
            {
                logger.Error("User with id {id} is not found!", request.UserId);
                return false;
            }
            DiagnoseData data = new()
            {
                Id = CreateHash(request.Id),
                Params = JsonSerializer.Deserialize<JsonDocument>(request.Request.ToString()),
                Result = request.Response.Answer,
                ResultValue = request.Response.Value ?? -1.0,
                User = user
            };
            await db.DiagnoseData.AddAsync(data);

            return await db.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> WriteLineAsync(DBWriteRequest request)
        {
            logger.Info("Write line request: {@request}", request);
            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive);
            if (user == null)
            {
                logger.Warn("User with id {id} is not found!", request.UserId);
                return false;
            }
            DataToStorage data = new()
            {
                Id = CreateHash(request.Id),
                Data = JsonSerializer.Deserialize<JsonDocument>(request.Request.ToString()),
                UserKey = request.UserId,
                User = user
            };
            await db.DataToStorage.AddAsync(data);

            return await db.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> WriteLinesAsync(DBWriteRequest request)
        {
            logger.Info("Write line request: {@request}", request);
            JsonDocument[] datas = JsonSerializer.Deserialize<JsonDocument[]>(request.Request.ToString());
            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive);
            if (user == null)
            {
                logger.Warn("User with id {id} is not found!", request.UserId);
                return false;
            }
            List<DataToStorage> dataToStorage = new();
            for(int i = 0; i < datas.Length; i++)
            {
                dataToStorage.Add(new()
                {
                    Id = CreateHash($"{i}_{request.Id}"),
                    Data = datas[i],
                    UserKey = request.UserId,
                    User = user
                });
            }
            await db.DataToStorage.AddRangeAsync(dataToStorage);

            return await db.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> WriteAsynchronousDiagnoseResults(DiagnoseResult result)
        {
            logger.Info("Write async diagnose results request: {@result}", result);
            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == result.UserId && u.IsActive);
            if (user == null)
            {
                logger.Warn("User with id {id} is not found!", result.UserId);
                return false;
            }
            string jsonData = JsonSerializer.Serialize(result.DiagnoseData.ToString());
            DiagnoseData dataToStorage = new()
            {
                Result = result.Result,
                ResultValue = result.ResultValue,
                SessionId = result.SessionId.Replace($"{result.UserId}_", ""),
                Params = JsonSerializer.Deserialize<JsonDocument>(jsonData),
                Id = CreateHash($"{jsonData}_{result.UserId}_{result.SessionId}_{Guid.NewGuid()}"),
                User = user
            };
            await db.DiagnoseData.AddAsync(dataToStorage);
            return await db.SaveChangesAsync() > 0;
        }

        private static string CreateHash(string str)
        {
            str += Guid.NewGuid().ToString();
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(str));
            return Convert.ToBase64String(hash);
        }

    }
}
