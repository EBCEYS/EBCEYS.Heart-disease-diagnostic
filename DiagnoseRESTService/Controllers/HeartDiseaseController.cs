using DiagnoseDataObjects;
using DiagnoseRestService.Server;
using DiagnoseRESTService.Models;
using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DiagnoseRestService.Controllers
{
    /// <summary>
    /// The heart disease controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class HeartDiseaseController : ControllerBase
    {
        /// <summary>
        /// The heart desease controller constructor.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataServer">The data server.</param>
        /// <param name="config">The configuration.</param>
        public HeartDiseaseController(Logger logger, DataServer dataServer, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;
            this._server = dataServer;
            _jsonSettings.Converters.Add(new JsonStringEnumConverter());
        }
        private readonly Logger logger;
        private readonly IConfiguration config;
        private readonly DataServer _server;
        private readonly JsonSerializerOptions _jsonSettings = new()
                            {
                                WriteIndented = false,
                                AllowTrailingCommas = true,
                                PropertyNameCaseInsensitive = true
                            };


        /// <summary>
        /// The main method. Uses to diagnose data by definite algorithm.
        /// </summary>
        /// <param name="algorithm">The AI algorithm.</param>
        /// <param name="dataSetType">The data set type.</param>
        /// <param name="data">The values set by dataset example.</param>
        /// <param name="requestId">The request id.</param>
        /// <returns>The action response.</returns>
        [ProducesResponseType(typeof(RestActionResponse), 200)]
        [ProducesResponseType(typeof(RestActionResponse), 400)]
        [ProducesResponseType(typeof(RestActionResponse), 500)]
        [Authorize(Roles = "adm,usr")]
        [HttpPost("{dataSetType}/diagnose")]
        public async Task<ActionResult<RestActionResponse>> Diagnose([Required][FromRoute] DataSetTypes dataSetType, [FromBody] JsonDocument data, [Required][FromHeader] string requestId)
        {
            logger.Info("public ActionResult<ActionResponse> Diagnose([Required][FromQuery] DataSetTypes {dataSetType}, [FromBody] JsonDocument {@data}, [Required][FromHeader] string {requestId})", dataSetType, data.RootElement.ToString(), requestId);
            string dataSet = data.RootElement.ToString();
            RestActionResponse response;
            if (string.IsNullOrEmpty(requestId))
            {
                response = new() { Answer = Result.ERROR_WRONG_REQUEST_ID };
                return BadRequest(response);
            }
            try
            {
                string userId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                List<string> nullVals;
                switch (dataSetType)
                {
                    case DataSetTypes.HeartFailurePredictionDataSet:
                        HeartFailurePredictionDataSet clevelandDataSet = JsonSerializer.Deserialize<HeartFailurePredictionDataSet>
                            (dataSet, _jsonSettings);

                        if (!clevelandDataSet.CheckAttributes(out nullVals))
                        {
                            logger.Error("Null values list: {@nullVals}", nullVals);
                            response = new() { Answer = Result.ERROR_WRONG_DATASET, RequestId = requestId, Value = null };
                            break;
                        }

                        clevelandDataSet.DataSetType = DataSetTypes.HeartFailurePredictionDataSet;
                        response = await DataSetExecuteAsync(requestId, clevelandDataSet, clevelandDataSet.DataSetType.Value, userId);
                        break;
                    case DataSetTypes.CardiovascularDiseaseDataSet:
                        CardiovascularDiseaseDataSet cardioDataSet = JsonSerializer.Deserialize<CardiovascularDiseaseDataSet>
                            (dataSet, _jsonSettings);

                        if (!cardioDataSet.CheckAttributes(out nullVals))
                        {
                            logger.Error("Null values list: {@nullVals}", nullVals);
                            response = new() { Answer = Result.ERROR_WRONG_DATASET, RequestId = requestId, Value = null };
                            break;
                        }

                        cardioDataSet.DataSetType = DataSetTypes.CardiovascularDiseaseDataSet;
                        response = await DataSetExecuteAsync(requestId, cardioDataSet, cardioDataSet.DataSetType.Value, userId);
                        break;
                    case DataSetTypes.MaleCardiovascularDiseaseDataSet:
                        MaleCardiovascularDiseaseDataSet maleDataSet = JsonSerializer.Deserialize<MaleCardiovascularDiseaseDataSet>
                            (dataSet, _jsonSettings);

                        if (!maleDataSet.CheckAttributes(out nullVals))
                        {
                            logger.Error("Null values list: {@nullVals}", nullVals);
                            response = new() { Answer = Result.ERROR_WRONG_DATASET, RequestId = requestId, Value = null };
                            break;
                        }

                        maleDataSet.DataSetType = DataSetTypes.MaleCardiovascularDiseaseDataSet;
                        response = await DataSetExecuteAsync(requestId, maleDataSet, maleDataSet.DataSetType.Value, userId);
                        break;
                    default:
                        response = new() { Answer = Result.ERROR_WRONG_DATASET, RequestId = requestId, Value = null };
                        break;
                }
            }
            catch (JsonException ex)
            {
                logger.Error(ex, "Parsing error", ex.Message);
                response = null;
            }
            if (response == null)
            {
                response = new() { Answer = Result.ERROR, RequestId = requestId, Value = null };
            }
            logger.Info("Response is {@response}", response);
            switch(response.Answer)
            {
                case Result.OK:
                    return Ok(response);
                case Result.ERROR:
                    return StatusCode(500, response);
                default:
                    return BadRequest(response);
            }
            
        }

        private async Task<RestActionResponse> DataSetExecuteAsync(string requestId, object clevelandDataSet, DataSetTypes dataSetTypes, string userId)
        {
            RestActionResponse response = await _server.RequestToCalcAsync(requestId, clevelandDataSet, dataSetTypes, userId);
            if (response != null)
            {
                response.RequestId = requestId;
            }

            return response;
        }

        /// <summary>
        /// The ping.
        /// </summary>
        /// <returns>Pong</returns>
        [ProducesResponseType(typeof(string), 200)]
        [AllowAnonymous]
        [HttpGet("/ping")]
        public ActionResult Ping()
        {
            logger.Debug("public ActionResult Ping()");
            return Ok("Pong");
        }

        /// <summary>
        /// Gets Heart Failure Prediction data set example.
        /// </summary>
        /// <returns>The Heart Failure Prediction data set example.</returns>
        [ProducesResponseType(typeof(HeartFailurePredictionDataSet), 200)]
        [Authorize(Roles = "adm,usr")]
        [HttpGet("/heart-failure-prediction-example")]
        public ActionResult<HeartFailurePredictionDataSet> GetHeartFailurePredictionExample()
        {
            logger.Info("public ActionResult GetClevelandExample()");
            HeartFailurePredictionDataSet example = new()
            {
                Age = 63,
                Sex = 1,
                ChestPainType = 3,
                RestingBloodPressure = 145,
                SerumCholestoral = 233,
                FastingBloodSugar = false,
                MaximumHeartRateAchieved = 150,
                RestingElectrocardiographicResults = 1,
                ExerciseInducedAngina = false,
                STDepression = 2.3,
                STSlope = STSlopeType.Flat,
                DataSetType = null
            };
            logger.Info("Result is {@example}", example);
            return Ok(example);
        }
        /// <summary>
        /// Gets Cardiovascular Disease data set example.
        /// </summary>
        /// <returns>The Cardiovascular Disease data set example.</returns>
        [ProducesResponseType(typeof(CardiovascularDiseaseDataSet), 200)]
        [Authorize(Roles = "adm,usr")]
        [HttpGet("/cardiovascular-disease-example")]
        public ActionResult<CardiovascularDiseaseDataSet> GetCardiovascularDiseaseExample()
        {
            logger.Info("public ActionResult GetCardiovascularDiseaseExample()");
            CardiovascularDiseaseDataSet example = new()
            {
                Age = 18393,
                Gender = 2,
                Height = 168,
                Weight = 62.0,
                SystolicBloodPressure = 110,
                DiastolicBloodPressure = 80,
                Cholesterol = 1,
                Glucose = 1,
                Smoking = false,
                AlcoholIntake = false,
                PhysicalActivity = true,
                DataSetType = null
            };
            logger.Info("Result is {@example}", example);
            return Ok(example);
        }
        /// <summary>
        /// Gets Male Cardiovascular Disease data set example.
        /// </summary>
        /// <returns>The Male Cardiovascular Disease data set example.</returns>
        [ProducesResponseType(typeof(MaleCardiovascularDiseaseDataSet), 200)]
        [Authorize(Roles = "adm,usr")]
        [HttpGet("/male-cardiovascular-disease-example")]
        public ActionResult<MaleCardiovascularDiseaseDataSet> GetMaleCardiovascularDiseaseExample()
        {
            logger.Info("public ActionResult GetMaleCardiovascularDiseaseExample()");
            MaleCardiovascularDiseaseDataSet example = new()
            {
                SystolicBloodPressure = 160,
                Tobacoo = 12,
                Cholesterol = 5.73,
                Adiposity = 23.11,
                FamilyHistory = true,
                TypeA = 49,
                Obesity = 25.3,
                Alcohol = 97.2,
                Age = 52,
                DataSetType = null
            };
            logger.Info("Result is {@example}", example);
            return Ok(example);
        }

        /// <summary>
        /// Writes client data set line to data base.
        /// </summary>
        /// <returns>TActionResponse</returns>
        [ProducesResponseType(typeof(RestActionResponse), 200)]
        [ProducesResponseType(typeof(RestActionResponse), 400)]
        [ProducesResponseType(typeof(RestActionResponse), 500)]
        [Authorize(Roles = "adm,usr")]
        [HttpPut("{dataSetName}/write-line")]
        public async Task<ActionResult<RestActionResponse>> WriteLine([Required][FromHeader] string requestId, [Required][FromRoute] string dataSetName, [Required][FromBody] JsonDocument jsonLine)
        {
            logger.Info("WriteLine([Required][FromHeader] string {requestId}, [Required][FromRoute] string {dataSetName}, [Required][FromBody] JsonDocument {jsonLine})", requestId, dataSetName, jsonLine.RootElement.ToString());
            RestActionResponse response;
            if (string.IsNullOrEmpty(requestId))
            {
                response = new()
                {
                    Answer = Result.ERROR_WRONG_REQUEST_ID,
                    RequestId = requestId
                };
            }
            else if (string.IsNullOrEmpty(dataSetName) || jsonLine is null)
            {
                response = new()
                {
                    Answer = Result.ERROR_WRONG_DATASET,
                    RequestId = requestId
                };
            }
            else
            {
                try
                {
                    string userId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    await _server.RequestToWriteLineAsync(requestId, dataSetName, jsonLine, userId);
                    response = new()
                    {
                        Answer = Result.OK,
                        RequestId = requestId
                    };
                }
                catch(Exception ex)
                {
                    logger.Error(ex, "Error on posting write data request!");
                    response = new()
                    {
                        Answer = Result.ERROR_CONNECTION,
                        RequestId = requestId
                    };
                }
            }
            logger.Info("Response is {@response}", response);
            switch(response.Answer)
            {
                case Result.OK:
                    return Ok(response);
                case Result.ERROR:
                    return StatusCode(500, response);
                default:
                    return BadRequest(response);
            }
        }

        /// <summary>
        /// Writes client data set lines to data base.
        /// </summary>
        /// <returns>ActionResponse</returns>
        [ProducesResponseType(typeof(RestActionResponse), 200)]
        [ProducesResponseType(typeof(RestActionResponse), 400)]
        [ProducesResponseType(typeof(RestActionResponse), 500)]
        [Authorize(Roles = "adm,usr")]
        [HttpPut("{dataSetName}/write-lines")]
        public async Task<ActionResult<RestActionResponse>> WriteLines([Required][FromHeader] string requestId, [Required][FromRoute] string dataSetName, [Required][FromBody] List<JsonDocument> jsonLines)
        {
            logger.Info("WriteLines([Required][FromHeader] string {requestId}, [Required][FromRoute] string {dataSetName}, [Required][FromBody] List<JsonDocument> {@jsonLine})", requestId, dataSetName, jsonLines);
            RestActionResponse response;
            if (string.IsNullOrEmpty(requestId))
            {
                response = new()
                {
                    Answer = Result.ERROR_WRONG_REQUEST_ID,
                    RequestId = requestId
                };
            }
            else if (string.IsNullOrEmpty(dataSetName) || jsonLines is null)
            {
                response = new()
                {
                    Answer = Result.ERROR_WRONG_DATASET,
                    RequestId = requestId
                };
            }
            else
            {
                try
                {
                    string userId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    await _server.RequestToWriteLinesAsync(requestId, dataSetName, jsonLines, userId);
                    response = new()
                    {
                        Answer = Result.OK,
                        RequestId = requestId
                    };
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error on posting write data request!");
                    response = new()
                    {
                        Answer = Result.ERROR_CONNECTION,
                        RequestId = requestId
                    };
                }
            }
            logger.Info("Response is {@response}", response);
            switch (response.Answer)
            {
                case Result.OK:
                    return Ok(response);
                case Result.ERROR:
                    return StatusCode(500, response);
                default:
                    return BadRequest(response);
            }
        }
        /// <summary>
        /// Gets the diagnose results from queue.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="refresh">true: wait until service get your diagnose results (max wait time is 30 sec); false - gets cached diagnose results.</param>
        /// <returns></returns>
        [HttpGet("diagnose/results/{sessionId}/{refresh}")]
        [Authorize]
        [ProducesResponseType(typeof(List<DiagnoseResultRESTService>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<DiagnoseResultRESTService>>> GetDiagnoseResults([Required][FromRoute] string sessionId, [Required][FromRoute] bool refresh)
        {
            logger.Info("Get GetDiagnoseResults request: {sessingid}, {refresh}", sessionId, refresh);
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return BadRequest();
                }
                string userId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                sessionId = $"{userId}_{sessionId}";
                List<DiagnoseResult> results = await _server.GetDiagnoseResults(sessionId, refresh) ?? new();
                List<DiagnoseResultRESTService> restResults = new();
                results.ForEach(x =>
                {
                    restResults.Add(DiagnoseResultRESTService.CreateFrom(x));
                });
                if (restResults.Any())
                {
                    return Ok(restResults);
                }
                return NotFound();
            }
            catch(Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
