using AuthService.ApiResponses;
using AuthService.Exceptions;
using CacheAdapters.CacheModels;
using CacheAdapters.JwtCache;
using CacheAdapters.UsersCache;
using DataBaseObjects.AlertDB;
using DataBaseObjects.RolesDB;
using DataBaseObjects.UsersDB;
using EBCEYS.RabbitMQ.Client;
using HeartDiseasesDiagnosticExtentions.AuthExtensions;
using HeartDiseasesDiagnosticExtentions.RabbitMQExtensions;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UsersContracts;

namespace AuthService.Server
{
    /// <summary>
    /// The data server.
    /// </summary>
    public class DataServer
    {

        private readonly Logger logger;
        private readonly IConfiguration config;
        private readonly IUsersCacheAdapter usersCache;
        private readonly IJwtCacheAdapter jwtCache;

        /// <summary>
        /// The rabbitMQ rpc client.
        /// </summary>
        public RabbitMQClient RpcClient { get; }

        private readonly long tokenExpireTime;
        private readonly TimeSpan refreshTokenLiveTime;

        /// <summary>
        /// The roles list.
        /// </summary>
        public string[] RolesList { get; set; }

        /// <summary>
        /// Creates the data server.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="client">The rabbit mq client.</param>
        /// <param name="usersCache">The users cache.</param>
        /// <param name="jwtCache">The jwt cache.</param>
        public DataServer(Logger logger, IConfiguration config, RabbitMQClient client, IUsersCacheAdapter usersCache, IJwtCacheAdapter jwtCache)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            logger.Info("Starting data server...");

            tokenExpireTime = this.config.GetValue<long?>("TokenExpireTime") ?? 3;
            refreshTokenLiveTime = TimeSpan.FromMinutes(this.config.GetValue<double?>("RefreshTokenLifeTime") ?? 5.0);

            RpcClient = client ?? throw new ArgumentNullException(nameof(client));
            this.usersCache = usersCache ?? throw new ArgumentNullException(nameof(usersCache));
            this.jwtCache = jwtCache ?? throw new ArgumentNullException(nameof(jwtCache));
            RolesList = GetRolesList();
            try
            {
                CreateNewRoleAsync(new()
                {
                    RoleName = "admin",
                    Roles = RolesList.ToList()
                }).Wait();
                RegisterAsync(new()
                {
                    UserName = "admin",
                    Password = "admin",
                    Role = "admin"
                }).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                this.logger.Error(ex, "Error on registing admin user!");
            }
        }

        private string[] GetRolesList()
        {
            try
            {
                return config.GetSection("Roles").Get<string[]>()!;
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on requesting roles!");
                return null;
            }
        }
        /// <summary>
        /// Sends login request to users database adapter.
        /// </summary>
        /// <param name="login">The login model.</param>
        /// <returns>The login response.</returns>
        public virtual async Task<LoginResponse> LoginAsync(LoginModel login)
        {
            if (login is null || string.IsNullOrEmpty(login.UserName) || string.IsNullOrEmpty(login.Password))
            {
                throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
            }

            UsersContractResponse<User> result = await RpcClient.SendRequestAsync<UsersContractResponse<User>>(new()
            {
                Method = "LoginUser",
                Params = new[] { login }
            }) 
                ?? throw new RabbitMQException("login rpc request result is null!");
            if (result.Result == UsersResponseContractResult.UserNotFound)
            {
                return new LoginResponse()
                {
                    ResponseResult = UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS
                };
            }
            if (result.Result == UsersResponseContractResult.Error)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }
            string token = GenerateToken(result.Object);
            string refreshToken = GenerateRefreshToken(result.Object);
            if (await usersCache.AddUserToCacheAsync(result.Object))
            {
                await jwtCache.AddJwtDataAsync(new(refreshToken, result.Object.Id, token, refreshTokenLiveTime));
                return new()
                {
                    ResponseResult = UsersResponseResult.OK,
                    Token = token ?? null,
                    UserName = result.Object.Name ?? null,
                    Role = result.Object.Role ?? null,
                    RefreshToken = refreshToken
                };
            }
            logger.Error("Can not add user to cache!");
            return new()
            {
                ResponseResult = UsersResponseResult.ERROR
            };
        }
        /// <summary>
        /// Refreshs the user async.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="currentToken">The current JWT.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<RefreshTokenResponse> RefreshUserAsync(string userId, string refreshToken, string currentToken)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"\"{nameof(userId)}\" не может быть пустым или содержать только пробел.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException($"\"{nameof(refreshToken)}\" не может быть пустым или содержать только пробел.", nameof(refreshToken));
            }

            if (string.IsNullOrWhiteSpace(currentToken))
            {
                throw new ArgumentException($"\"{nameof(currentToken)}\" не может быть пустым или содержать только пробел.", nameof(currentToken));
            }

            return await ProcessUsersRefreshAsync(userId, refreshToken, currentToken);
        }

        private async Task<RefreshTokenResponse> ProcessUsersRefreshAsync(string userId, string refreshToken, string currentToken)
        {
            JwtCachedData? data = await jwtCache.GetJwtDataAsync(refreshToken);
            if (data == null)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.REFRESH_TOKEN_IS_EXPIRED_OR_DOES_NOT_EXISTS
                };
            }
            if (data.UserId != userId || data.CurrentToken != currentToken)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_WRONG_PARAMS
                };
            }
            User? user = await usersCache.GetUserFromCacheAsync(userId);
            if (user == null)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.USER_IS_NOT_AUTHORIZED
                };
            }
            string newToken = GenerateToken(user);
            string newRefreshToken = GenerateRefreshToken(user);
            if (await jwtCache.RemoveJwtDataAsync(refreshToken))
            {
                await jwtCache.AddJwtDataAsync(new()
                {
                    RefreshToken = newRefreshToken,
                    CurrentToken = newToken,
                    UserId = userId
                });
            }
            else
            {
                logger.Warn("Error on removing old refresh token: {refresh}", refreshToken);
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR
                };
            }
            return new()
            {
                RefreshToken = newRefreshToken,
                Token = newToken,
                UserId = userId,
                UserName = user.Name,
                ResponseResult = UsersResponseResult.OK
            };
        }

        private static string GenerateRefreshToken(User user)
        {
            return $"{Guid.NewGuid():N}_{user.Name!}";
        }

        private static string CreateHash(string input)
        {
            byte[] hashed = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashed);
        }

        private string GenerateToken(User user)
        {
            if (string.IsNullOrEmpty(user.Name))
            {
                throw new Exception("Empty user name property on server response!");
            }
            if (!user.Role.Roles.Any())
            {
                throw new Exception("Empty role list!");
            }
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            foreach (string role in user.Role.Roles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }
            ClaimsIdentity claimsIdentity = new(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            JwtSecurityToken jwt = new(
                issuer: config["JwtAuth:Issuer"],
                audience: config["JwtAuth:Issuer"],
                claims: claimsIdentity.Claims,
                expires: DateTime.Now.AddMinutes(tokenExpireTime),
                signingCredentials:
                new SigningCredentials(new SymmetricSecurityKey(Program.SecretKey), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        /// <summary>
        /// Logouts user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public async Task<BaseAuthResponse> LogoutUserAsync(string userId)
        {
            if (await LogoutUserFromSystemAsync(userId))
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.OK
                };
            }
            return new()
            {
                ResponseResult = UsersResponseResult.USER_IS_NOT_AUTHORIZED
            };
        }

        /// <summary>
        /// The register rpc request.
        /// </summary>
        /// <param name="model">The registration model.</param>
        /// <returns>The registration response.</returns>
        public virtual async Task<RegistrationResponse> RegisterAsync(RegisterModel model)
        {
            if (model is null || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password) 
                || model.Role is null || !model.Role.Any())
            {
                throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
            }
            if (!IsRolesListCreated())
            {
                throw new AuthRestException(UsersResponseResult.ERROR);
            }

            UsersContractResponse<User> response = await RpcClient.SendRequestAsync<UsersContractResponse<User>>(new()
            {
                Method = "AddUser",
                Params = new[] { new User()
                    {
                        Name = model.UserName,
                        Password = model.Password,
                        Role = new()
                        {
                            RoleName = model.Role
                        }
                    }
                }
            }) ?? throw new RabbitMQException("Error on registration request! Empty response!");
            if (response.Result == UsersResponseContractResult.UserAlreadyExists)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_USER_ALREADY_EXISTS
                };
            }
            if (response.Result == UsersResponseContractResult.Error)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR
                };
            }
            return new()
            {
                ResponseResult = UsersResponseResult.OK,
                Role = response.Object?.Role ?? null
            };
        }

        private bool IsRolesListCreated()
        {
            return RolesList is not null && RolesList.Any();
        }

        /// <summary>
        /// Pings the users db adapter service.
        /// </summary>
        /// <returns>The ping result as string.</returns>
        public virtual async Task<string> PingAsync()
        {
            try
            {
                string result = await RpcClient.SendRequestAsync<string>(new()
                {
                    Method = "Ping"
                });

                if (result is null)
                {
                    return "Error";
                }
                else
                {
                    return result;
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on ping adapter!");
                return "Error";
            }
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="model">The delete user model.</param>
        /// <returns>The base response.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<BaseAuthResponse> DeleteUserAsync(DeleteModel model)
        {
            if (model is null || string.IsNullOrEmpty(model.UserId))
            {
                throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
            }

            UsersContractResponse<User> result = await RpcClient.SendRequestAsync<UsersContractResponse<User>>(new()
            {
                Method = "RemoveUser",
                Params = new[] { model.UserId }
            }) ?? throw new RabbitMQException("Error on rpc requesting delete!");
            if (result.Result == UsersResponseContractResult.UserNotFound)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS
                };
            }
            if (result.Result == UsersResponseContractResult.Error)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }
            await LogoutUserFromSystemAsync(model.UserId);
            return new()
            {
                UserName = result.Object.Name,
                ResponseResult = UsersResponseResult.OK
            };
        }

        /// <summary>
        /// Updates the user roles.
        /// </summary>
        /// <param name="model">The update user role model.</param>
        /// <returns>The base response.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<BaseAuthResponse> UpdateRolesAsync(UpdateRolesModel model)
        {
            if (model is null || string.IsNullOrEmpty(model.UserId))
            {
                throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
            }

            if (model.NewRole is null || !model.NewRole.Any())
            {
                throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
            }

            if (!IsRolesListCreated())
            {
                throw new AuthRestException(UsersResponseResult.ERROR_CONNECTION);
            }

            UsersContractResponse<User> result = await RpcClient.SendRequestAsync<UsersContractResponse<User>>(new()
            {
                Method = "UpdateUserRoles",
                Params = new[] 
                { 
                    model.UserId, 
                    model.NewRole
                }
            }) 
                ?? throw new RabbitMQException("Error on rpc requesting update users roles!");
            if (result.Result == UsersResponseContractResult.UserNotFound)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS
                };
            }
            await LogoutUserFromSystemAsync(model.UserId!);
            return new()
            {
                ResponseResult = UsersResponseResult.OK,
                UserName = result.Object.Name
            };
        }

        /// <summary>
        /// Gets the user id async.
        /// </summary>
        /// <param name="userName">The get userid model.</param>
        /// <returns>Base auth response.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="RabbitMQException"></exception>
        public virtual async Task<BaseAuthResponse> GetUserIdAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
            }

            UsersContractResponse<User> result = await RpcClient.SendRequestAsync<UsersContractResponse<User>>(new()
            {
                Method = "GetUserByName",
                Params = new[] { userName }
            }) 
                ?? throw new RabbitMQException("Error on rpc requesting user id!");
            if (result.Result == UsersResponseContractResult.UserNotFound)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS
                };
            }
            return new()
            {
                ResponseResult = UsersResponseResult.OK,
                UserId = string.IsNullOrEmpty(result.Object.Id) ? null : result.Object.Id,
                UserName = userName
            };
        }
        /// <summary>
        /// Gets the user alerts.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        /// <exception cref="AuthRestException"></exception>
        public virtual async Task<UserAlertsResponse> GetUserAlertsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new AuthRestException(UsersResponseResult.ERROR_WRONG_PARAMS);
            }

            UsersContractResponse<List<Alert>> alerts = await RpcClient.SendRequestAsync<UsersContractResponse<List<Alert>>>(new()
            {
                Method = "GetUserAlerts",
                Params = new[] { userId }
            });

            if (alerts == null)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }

            if (alerts.Result == UsersResponseContractResult.UserNotFound)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS
                };
            }

            return new()
            {
                ResponseResult = alerts.Object.Any() ? UsersResponseResult.OK : UsersResponseResult.ALERTS_ARE_NOT_EXISTS,
                Alerts = alerts.Object ?? null,
                UserId = userId
            };
        }
        /// <summary>
        /// Changes user's password.
        /// </summary>
        /// <param name="model">The change user's password model.</param>
        /// <returns></returns>
        public async Task<BaseAuthResponse> ChangeUsersPasswordAsync(ChangePasswordApiModel model)
        {
            UsersContractResponse<object?> response = await RpcClient.SendRequestAsync<UsersContractResponse<object?>>(new()
            {
                Method = "ChangePassword",
                Params = new[] { model }
            });
            if (response == null || response.Result == UsersResponseContractResult.Error)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }
            if (response.Result == UsersResponseContractResult.Ok)
            {
                await LogoutUserFromSystemAsync(model.UserId!);
            }
            return response.Result switch
            {
                UsersResponseContractResult.UserNotFound => new()
                {
                    ResponseResult = UsersResponseResult.ERROR_USER_DOES_NOT_EXISTS
                },
                UsersResponseContractResult.Ok => new()
                {
                    ResponseResult = UsersResponseResult.OK
                },
                _ => new()
                {
                    ResponseResult = UsersResponseResult.ERROR
                }
            };
        }

        private async Task<bool> LogoutUserFromSystemAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException($"\"{nameof(userId)}\" не может быть пустым или содержать только пробел.", nameof(userId));
            }

            return await usersCache.RemoveUserFromCacheAsync(userId) || await jwtCache.RemoveJwtDataByUserAsync(userId);
        }
        /// <summary>
        /// Creates the new role.
        /// </summary>
        /// <param name="newRole">The new role.</param>
        /// <returns></returns>
        public async Task<BaseAuthResponse> CreateNewRoleAsync(UserRole newRole)
        {
            if (newRole is null || string.IsNullOrWhiteSpace(newRole.RoleName) || newRole.Roles == null || !newRole.Roles.Any())
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_WRONG_PARAMS
                };
            }

            UsersContractResponse<object> result = await RpcClient.SendRequestAsync<UsersContractResponse<object>>(new()
            {
                Method = "CreateNewRole",
                Params = new[] { newRole }
            });

            if (result == null)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }

            return result.Result switch
            {
                UsersResponseContractResult.Ok => new()
                {
                    ResponseResult = UsersResponseResult.OK
                },
                UsersResponseContractResult.RoleAlreadyExists => new()
                {
                    ResponseResult = UsersResponseResult.ERROR_WRONG_ROLE
                },
                _ => new()
                {
                    ResponseResult = UsersResponseResult.ERROR
                }
            };
        }
        /// <summary>
        /// Gets the roles list.
        /// </summary>
        /// <returns></returns>
        public async Task<RolesListResponse> GetRolesListAsync()
        {
            UsersContractResponse<List<UserRole>> result = await RpcClient.SendRequestAsync<UsersContractResponse<List<UserRole>>>(new()
            {
                Method = "GetRolesList"
            });
            if (result == null)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }
            return result.Result switch
            {
                UsersResponseContractResult.Ok => new()
                {
                    ResponseResult = UsersResponseResult.OK,
                    Roles = result.Object ?? new()
                },
                UsersResponseContractResult.RoleDoesNotExist => new()
                {
                    ResponseResult = UsersResponseResult.ERROR_ROLE_DOES_NOT_EXISTS
                },
                _ => new()
                {
                    ResponseResult = UsersResponseResult.ERROR
                }
            };
        }
        /// <summary>
        /// Removes role async.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <returns></returns>
        public async Task<BaseAuthResponse> RemoveRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_WRONG_PARAMS
                };
            }
            UsersContractResponse<object> result = await RpcClient.SendRequestAsync<UsersContractResponse<object>>(new()
            {
                Method = "RemoveRole",
                Params = new[] { roleName }
            });
            if (result == null)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }
            return result.Result switch
            {
                UsersResponseContractResult.Ok => new()
                {
                    ResponseResult = UsersResponseResult.OK
                },
                UsersResponseContractResult.RoleDoesNotExist => new()
                {
                    ResponseResult = UsersResponseResult.ERROR_ROLE_DOES_NOT_EXISTS
                },
                _ => new()
                {
                    ResponseResult = UsersResponseResult.ERROR
                }
            };
        }
        /// <summary>
        /// Updates the role.
        /// </summary>
        /// <param name="role">The role to update.</param>
        /// <returns></returns>
        public async Task<BaseAuthResponse> UpdateRoleAsync(UserRole role)
        {
            if (role is null || string.IsNullOrWhiteSpace(role.RoleName) || role.Roles == null || !role.Roles.Any())
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_WRONG_PARAMS
                };
            }
            UsersContractResponse<object> result = await RpcClient.SendRequestAsync<UsersContractResponse<object>>(new()
            {
                Method = "UpdateRole",
                Params = new[] { role }
            });
            if (result == null)
            {
                return new()
                {
                    ResponseResult = UsersResponseResult.ERROR_CONNECTION
                };
            }
            return result.Result switch
            {
                UsersResponseContractResult.Ok => new()
                { 
                    ResponseResult = UsersResponseResult.OK 
                },
                UsersResponseContractResult.RoleDoesNotExist => new()
                { 
                    ResponseResult = UsersResponseResult.ERROR_ROLE_DOES_NOT_EXISTS 
                },
                _ => new()
                {
                    ResponseResult = UsersResponseResult.ERROR
                }
            };
        }
    }
}
