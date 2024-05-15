using DiagnoseDataObjects;
using EBCEYS.RabbitMQ.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiagnoseReceiver.Controllers
{
    /// <summary>
    /// Diagnose receiver controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DiagnoseReceiverController : ControllerBase
    {
        private readonly ILogger<DiagnoseReceiverController> logger;
        private readonly RabbitMQClient client;

        /// <summary>
        /// Initiates the new instance of the diagnose receiver controller.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="client">The rabbitMQ client.</param>
        public DiagnoseReceiverController(ILogger<DiagnoseReceiverController> logger, RabbitMQClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        /// <summary>
        /// Posts the data to diagnose.
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Authorize(Roles = "usr, adm")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> Post(InputWetData data)
        {
            if (data is null)
            {
                return BadRequest("Your data is null!");
            }

            try
            {
                string userId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                PrepairedWetData prepData = new()
                {
                    InputData = new()
                    {
                        DataToDiagnose = data.DataToDiagnose,
                        DataType = data.DataType,
                        SessionId = $"{userId}_{data.SessionId}"
                    },
                    UserId = userId
                };
                logger.LogDebug("Try to send prepaierd data to queue: {@data}", prepData);
                await client.SendMessageAsync(new()
                {
                    Method = "WetData",
                    Params = new [] { prepData }
                });
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on posting data to rabbitMQ!");
                return StatusCode(500);
            }
        }
    }
}