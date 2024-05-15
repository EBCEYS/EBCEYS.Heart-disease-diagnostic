using HolterAnalyzeRest.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HolterAnalyzeRest.Controllers
{
    /// <summary>
    /// The holter analyze controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class HolterController : Controller
    {
        private readonly Logger logger;
        private readonly IConfiguration config;
        private readonly DataServer dataServer;
        private readonly string fileExtension;
        /// <summary>
        /// Creates the new holter analyze controller.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="dataServer">The data server.</param>
        public HolterController(Logger logger, IConfiguration config, DataServer dataServer)
        {
            this.logger = logger;
            this.config = config;
            this.dataServer = dataServer;

            fileExtension = this.config.GetSection("FileExtension").Value ?? throw new Exception("Error on reading file extension!");
        }

        /// <summary>
        /// The ping.
        /// </summary>
        /// <returns>The pong.</returns>
        /// <response code="200">Successful ping.</response>
        [HttpGet("ping")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), 200)]
        public ActionResult<string> Ping()
        {
            logger.Info("Respose is Pong!");
            return Ok("Pong!");
        }

        /// <summary>
        /// The analyze method.
        /// </summary>
        /// <param name="file">The file. File size limit is 209715200.</param>
        /// <returns></returns>
        /// <response code="200">Successfuly analyzed.</response>
        /// <response code="400">File is null or wrong file extension.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="502">Algorithm error. See <see cref="AnalyzeResponse.ExceptionMessage"/> in response object.</response>
        [HttpPost("analyze")]
        [Authorize(Roles = "usr")]
        //[AllowAnonymous]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        [ProducesResponseType(typeof(AnalyzeResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [ProducesResponseType(typeof(AnalyzeResponse), 502)]
        public async Task<ActionResult> Analyze([Required]IFormFile file)
        {
            logger.Info("public ActionResult Analyze(IFormFile {file})", file.FileName);

            if (file is null)
            {
                return BadRequest("File is null!");
            }

            if (string.Compare(fileExtension, Path.GetExtension(file.FileName), StringComparison.OrdinalIgnoreCase) != 0)
            {
                return BadRequest($"Wrong file extension! File extension should be \"{fileExtension}\"!");
            }

            using MemoryStream stream = new();

            await file.CopyToAsync(stream);

            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                string userName = User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value ?? throw new Exception("Username is null!");

                AnalyzeResponse response = await dataServer.UploadDataAsync(stream, file.FileName, userName);

                logger.Info("Response is {@response}", response.ExceptionMessage ?? "Ok");
                if (string.IsNullOrEmpty(response.ExceptionMessage))
                {
                    return Ok(response);
                }
                else
                {
                    response.Files = null;
                    response.Values = null;
                    return StatusCode(502, response);
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on uploading data!");
                return StatusCode(500, $"Something went wrong! {ex.Message}");
            }
        }

    }
}
