using AuthService.ApiResponses;
using AuthService.Exceptions;
using AuthService.Server;
using DataBaseObjects.RolesDB;
using HeartDiseasesDiagnosticExtentions.AuthExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace AuthService.Controllers
{
    /// <summary>
    /// The auth controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly Logger logger;
        private readonly DataServer dataServer;

        /// <summary>
        /// Initiates the auth controller.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataServer">The data server.</param>
        public AuthController(Logger logger, DataServer dataServer)
        {
            this.logger = logger;
            this.dataServer = dataServer;
        }

        /// <summary>
        /// The Ping.
        /// </summary>
        /// <returns>Pong</returns>
        /// <response code="200">Successful ping.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("Ping")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public ActionResult<string> Ping()
        {
            logger.Info("public string Ping()");
            string response;
            if (dataServer == null || dataServer?.RpcClient == null)
            {
                response = "Not pong!";
                logger.Info("Response is {response}", response);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            response = "Pong";
            logger.Info("Response is {response}", response);
            return Ok(response);
        }

        /// <summary>
        /// Pings the db adapter.
        /// </summary>
        /// <returns>The db adapter status.</returns>
        /// <response code="200">Successful ping adapter..</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("PingAdapter")]
        [Authorize(Roles = "adm")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<ActionResult<string>> PingAdapter()
        {
            logger.Info("public string PingAdapter()");

            string response = await dataServer.PingAsync();
            logger.Info("Response is {response}", response);
            if (response.StartsWith("E"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            return Ok(response);
        }

        /// <summary>
        /// The login request.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns>The login response.</returns>
        /// <response code="200">Successful login.</response>
        /// <response code="401">Unsuccessful login.</response>
        [HttpPost("user/login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(LoginResponse), 401)]
        public async Task<ActionResult<LoginResponse>> Login([Required][FromBody] LoginModel loginModel)
        {
            logger.Info("Login request {@loginModel}", loginModel);
            LoginResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };
            try
            {
                response = await dataServer.LoginAsync(loginModel) ?? response;
            }
            catch (AuthRestException authException)
            {
                response.ResponseResult = authException.Result;
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on login request!");
            }
            logger.Info("Response is {@response}", response);
            return response.ResponseResult switch
            {
                UsersResponseResult.OK => (ActionResult<LoginResponse>)Ok(response),
                UsersResponseResult.ERROR_WRONG_PARAMS => BadRequest(response),
                _ => (ActionResult<LoginResponse>)Unauthorized(response),
            };
        }
        /// <summary>
        /// Refresh the user's JWT.
        /// </summary>
        /// <param name="data">The refresh token model.</param>
        /// <response code="200">Ok!</response>
        /// <response code="400">Wrong request params.</response>
        /// <response code="401">User is not authorized.</response>
        /// <response code="404">Refresh token is expired or does not exists.</response>
        [HttpPost("token/refresh")]
        [Authorize]
        [ProducesResponseType(typeof(RefreshTokenResponse), 200)]
        [ProducesResponseType(typeof(RefreshTokenResponse), 400)]
        [ProducesResponseType(typeof(RefreshTokenResponse), 401)]
        [ProducesResponseType(typeof(RefreshTokenResponse), 404)]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([Required][FromBody] RefreshTokenModel data)
        {
            RefreshTokenResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    logger.Warn("Can not get user id from JWT {token}!", data.RefreshToken);
                    response.ResponseResult = UsersResponseResult.ERROR_WRONG_PARAMS;
                    return BadRequest(response);
                }
                string? currentToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                if (string.IsNullOrEmpty(currentToken))
                {
                    logger.Warn("Can not get current user's JWT by request: {req}", data.RefreshToken);
                    response.ResponseResult = UsersResponseResult.USER_IS_NOT_AUTHORIZED;
                    return Unauthorized(response);
                }
                response = await dataServer.RefreshUserAsync(userId, data.RefreshToken!, currentToken);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on refreshing user!");
            }
            return response.ResponseResult switch
            {
                UsersResponseResult.OK => Ok(response),
                UsersResponseResult.REFRESH_TOKEN_IS_EXPIRED_OR_DOES_NOT_EXISTS => NotFound(response),
                UsersResponseResult.USER_IS_NOT_AUTHORIZED => Unauthorized(response),
                _ => BadRequest(response),
            };
        }
        /// <summary>
        /// Logouts the user.
        /// </summary>
        /// <param name="userId">The user id [optional].</param>
        /// <response code="200">Ok!</response>
        /// <response code="400">Bad params!</response>
        /// <response code="401">User is not authorized!</response>
        /// <exception cref="AuthRestException"></exception>
        [HttpPost("user/logout")]
        [Authorize]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 400)]
        [ProducesResponseType(typeof(BaseAuthResponse), 401)]
        public async Task<ActionResult<BaseAuthResponse>> Logout([FromQuery] string? userId)
        {
            BaseAuthResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
                }
                response = await dataServer.LogoutUserAsync(userId);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on logouting user {id}!", userId);
            }
            return response.ResponseResult switch
            {
                UsersResponseResult.OK => Ok(response),
                UsersResponseResult.USER_IS_NOT_AUTHORIZED => Unauthorized(response),
                _ => BadRequest(response)
            };
        }

        /// <summary>
        /// The registration request.
        /// </summary>
        /// <param name="model">The registration model.</param>
        /// <response code="200">Successful registration.</response>
        /// <response code="401">Unsuccessful registration.</response>
        /// <response code="500">Internal server error.</response>
        /// <returns></returns>
        [HttpPost("user/registration")]
        [Authorize(Roles = "adm")]
        [ProducesResponseType(typeof(RegistrationResponse), 200)]
        [ProducesResponseType(typeof(RegistrationResponse), 401)]
        [ProducesResponseType(typeof(RegistrationResponse), 500)]
        public async Task<ActionResult<RegistrationResponse>> Registration([Required][FromBody] RegisterModel model)
        {
            logger.Info("public ActionResult Register([Required][FromBody] RegisterModel {@model})", model);
            RegistrationResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };
            try
            {
                response = await dataServer.RegisterAsync(model);
            }
            catch(AuthRestException authException)
            {
                response.ResponseResult = authException.Result;
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on registration request!");
            }
            logger.Info("Response is {@response}", response);
            return response.ResponseResult switch
            {
                UsersResponseResult.OK => Ok(response),
                UsersResponseResult.ERROR_USER_ALREADY_EXISTS => BadRequest(response),
                UsersResponseResult.ERROR_WRONG_PARAMS => BadRequest(response),
                _ => StatusCode((int)HttpStatusCode.InternalServerError, response)
            };
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="model">The delete user model.</param>
        /// <returns>The base response.</returns>
        /// <response code="200">Successfuly deleted.</response>
        /// <response code="404">User is not exists.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("user/delete")]
        [Authorize(Roles="adm")]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 404)]
        [ProducesResponseType(typeof(BaseAuthResponse), 500)]
        public async Task<ActionResult<BaseAuthResponse>> DeleteUser([Required][FromBody] DeleteModel model)
        {
            logger.Info("public ActionResult<BaseResponse> DeleteUser([Requiered][FromBody] DeleteModel {@model})", model);
            BaseAuthResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };
            try
            {
                response = await dataServer.DeleteUserAsync(model);
            }
            catch (AuthRestException ex)
            {
                response.ResponseResult = ex.Result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on delete user request!");
            }

            logger.Info("Response is {@response}", response);

            return response.ResponseResult switch
            {
                UsersResponseResult.OK => Ok(response),
                UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS => NotFound(response),
                UsersResponseResult.ERROR_WRONG_PARAMS => BadRequest(response),
                _ => StatusCode((int)HttpStatusCode.InternalServerError, response)
            };
        }

        /// <summary>
        /// Changes user role.
        /// </summary>
        /// <param name="model">The update user role model.</param>
        /// <returns>The base response.</returns>
        /// <response code="200">Successfuly changed.</response>
        /// <response code="401">Wrong requestor role.</response>
        /// <response code="404">User is not exists.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("user/change/roles")]
        [Authorize(Roles = "adm")]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 400)]
        [ProducesResponseType(typeof(BaseAuthResponse), 404)]
        [ProducesResponseType(typeof(BaseAuthResponse), 500)]
        public async Task<ActionResult<BaseAuthResponse>> ChangeUserRoles([Required][FromBody] UpdateRolesModel model)
        {
            logger.Info("public ActionResult<BaseResponse> UpdateUserRoles([Required][FromBody] UpdateRolesModel {@model})", model);
            BaseAuthResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };

            try
            {
                response = await dataServer.UpdateRolesAsync(model);
            }
            catch(AuthRestException ex)
            {
                response.ResponseResult = ex.Result;
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on change user roles request!");
            }

            logger.Info("Response is {@response}", response);

            return response.ResponseResult switch
            {
                UsersResponseResult.OK => (ActionResult<BaseAuthResponse>)Ok(response),
                UsersResponseResult.ERROR_WRONG_PARAMS => BadRequest(response),
                UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS => (ActionResult<BaseAuthResponse>)NotFound(response),
                UsersResponseResult.ERROR_WRONG_ROLE => (ActionResult<BaseAuthResponse>)BadRequest(response),
                _ => (ActionResult<BaseAuthResponse>)StatusCode(500, response),
            };
        }
        /// <summary>
        /// Changes user's password.
        /// </summary>
        /// <param name="model">The change password api model.</param>
        /// <response code="200">Password changed.</response>
        /// <response code="404">Wrong current password or userid.</response>
        /// <response code="500">Internal server error!</response>
        [HttpPost("user/change/password")]
        [Authorize]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 404)]
        [ProducesResponseType(typeof(BaseAuthResponse), 500)]
        public async Task<ActionResult<BaseAuthResponse>> ChangeUsersPassword([Required][FromBody] ChangePasswordApiModel model)
        {
            BaseAuthResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };
            try
            {
                if (string.IsNullOrWhiteSpace(model.UserId))
                {
                    model.UserId = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                }
                response = await dataServer.ChangeUsersPasswordAsync(model);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on changing user's password! {userId}", model.UserId ?? "unknown");
            }
            return response.ResponseResult switch
            {
                UsersResponseResult.OK => Ok(response),
                UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS => NotFound(response),
                _ => StatusCode(500, response)
            };
        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The user id.</returns>
        /// <response code="200">Successfuly executed.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="404">User name not found.</response>
        /// <response code="500">Something went wrong...</response>
        [HttpGet("user/id")]
        [Authorize(Roles = "adm")]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 400)]
        [ProducesResponseType(typeof(BaseAuthResponse), 404)]
        [ProducesResponseType(typeof(BaseAuthResponse), 500)]
        public async Task<ActionResult<BaseAuthResponse>> GetUserId([Required][FromQuery] string userName)
        {
            logger.Info("public async Task<ActionResult<BaseResponse>> GetUserId([Required][FromQuery] GetUserIdModel {@userName})", userName);
            BaseAuthResponse response = new()
            {
                ResponseResult = UsersResponseResult.ERROR_CONNECTION
            };

            try
            {
                response = await dataServer.GetUserIdAsync(userName);
            }
            catch(AuthRestException ex)
            {
                response.ResponseResult = ex.Result;
            }
            catch (Exception ex) 
            {
                logger.Error(ex, "Error on get user id request!");
            }

            logger.Info("Response is {@response}", response);

            return response.ResponseResult switch
            {
                UsersResponseResult.OK => Ok(response),
                UsersResponseResult.ERROR_WRONG_PARAMS => BadRequest(response),
                UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS => NotFound(response),
                UsersResponseResult.ERROR_CONNECTION => StatusCode(500, response),
                _ => BadRequest(response)
            };
        }
        /// <summary>
        /// Gets the user alerts.
        /// </summary>
        /// <response code="200">Successfuly executed.</response>
        /// <response code="400">Bad request.</response>
        /// <response code="404">Alerts or user are not found!.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("user/alerts")]
        [Authorize]
        [ProducesResponseType(typeof(UserAlertsResponse), 200)]
        [ProducesResponseType(typeof(UserAlertsResponse), 400)]
        [ProducesResponseType(typeof(UserAlertsResponse), 404)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<ActionResult<UserAlertsResponse>> GetUserAlers()
        {
            try
            {
                UserAlertsResponse result = await dataServer.GetUserAlertsAsync(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
                if (result.ResponseResult == UsersResponseResult.ALERTS_ARE_NOT_EXISTS)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (AuthRestException ex)
            {
                return BadRequest(new UserAlertsResponse()
                {
                    ResponseResult = ex.Result
                });
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on getting user's alerts!");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Creates the new role.
        /// </summary>
        /// <param name="newRole">The new role to create.</param>
        /// <response code="200">Role created!</response>
        /// <response code="400">New role model is not filled well!</response>
        /// <response code="500">Internal server error!</response>
        [HttpPost("role")]
        [Authorize(Roles = "adm")]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 400)]
        [ProducesResponseType(typeof(BaseAuthResponse), 500)]
        public async Task<ActionResult<BaseAuthResponse>> CreateRole([Required][FromBody] UserRole newRole)
        {
            try
            {
                BaseAuthResponse result = await dataServer.CreateNewRoleAsync(newRole);
                if (result.ResponseResult == UsersResponseResult.OK)
                {
                    return Ok(result);
                }
                if (result.ResponseResult == UsersResponseResult.ERROR_WRONG_PARAMS)
                {
                    return BadRequest(result);
                }
                if (result.ResponseResult == UsersResponseResult.ERROR_WRONG_ROLE)
                {
                    return BadRequest(result);//podumat kakoy status code podhodit lu4we. Eta hren voznikaet esli takaya role uje sushestvuet.
                }
                return StatusCode(500, result);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on creating new role!");
                return StatusCode(500);
            }
        }
        /// <summary>
        /// Gets the roles list.
        /// </summary>
        /// <response code="200">Roles list.</response>
        /// <response code="404">Roles list is empty!</response>
        /// <response code="500">Internal server error!</response>
        [HttpGet("roles")]
        [Authorize]
        [ProducesResponseType(typeof(RolesListResponse), 200)]
        [ProducesResponseType(typeof(RolesListResponse), 404)]
        [ProducesResponseType(typeof(RolesListResponse), 500)]
        public async Task<ActionResult<RolesListResponse>> GetRolesList()
        {
            try
            {
                RolesListResponse result = await dataServer.GetRolesListAsync();
                return result.ResponseResult switch
                {
                    UsersResponseResult.OK => Ok(result),
                    UsersResponseResult.ERROR_ROLE_DOES_NOT_EXISTS => NotFound(result),
                    _ => StatusCode(500, result)
                };
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on getting roles list!");
                return StatusCode(500);
            }
        }
        /// <summary>
        /// Deletes the role.
        /// </summary>
        /// <param name="role">The role name.</param>
        /// <response code="200">Roles deleted.</response>
        /// <response code="404">Role does not exist.</response>
        /// <response code="500">Internal server error!</response>
        [HttpDelete("role/{role}")]
        [Authorize(Roles = "adm")]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 404)]
        [ProducesResponseType(typeof(BaseAuthResponse), 500)]
        public async Task<ActionResult<BaseAuthResponse>> RemoveRole([Required][FromRoute] string role)
        {
            try
            {
                BaseAuthResponse result = await dataServer.RemoveRoleAsync(role);
                return result.ResponseResult switch
                {
                    UsersResponseResult.OK => Ok(result),
                    UsersResponseResult.ERROR_ROLE_DOES_NOT_EXISTS => NotFound(result),
                    _ => StatusCode(500, result)
                };
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on removing role!");
                return StatusCode(500);
            }
        }
        /// <summary>
        /// Updates the role.
        /// </summary>
        /// <param name="role">The role name.</param>
        /// <param name="newRoles">The new roles.</param>
        /// <response code="200">Ok!</response>
        /// <response code="400">Wrong params!</response>
        /// <response code="404">Role does not exist!</response>
        /// <response code="500">Internal server error!</response>
        [HttpPut("role/{role}")]
        [Authorize(Roles = "adm")]
        [ProducesResponseType(typeof(BaseAuthResponse), 200)]
        [ProducesResponseType(typeof(BaseAuthResponse), 400)]
        [ProducesResponseType(typeof(BaseAuthResponse), 404)]
        [ProducesResponseType(typeof(BaseAuthResponse), 500)]
        public async Task<ActionResult<BaseAuthResponse>> UpdateRole([Required][FromRoute] string role, [Required][FromBody] string[] newRoles)
        {
            try
            {
                BaseAuthResponse result = await dataServer.UpdateRoleAsync(new()
                {
                    RoleName = role,
                    Roles = newRoles.ToList()
                });
                return result.ResponseResult switch
                {
                    UsersResponseResult.OK => Ok(result),
                    UsersResponseResult.ERROR_WRONG_PARAMS => BadRequest(result),
                    UsersResponseResult.ERROR_ROLE_DOES_NOT_EXISTS => NotFound(result),
                    _ => StatusCode(500, result)
                };
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on updating role! {roleName} {@roles}", role, newRoles);
                return StatusCode(500);
            }
        }

    }
}