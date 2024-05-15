using AuthService.Server;
using CacheAdapters.UsersCache;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using HeartDiseasesDiagnosticExtentions.RabbitMQExtensions;
using JWTExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace AuthService
{
    /// <summary>
    /// The startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            rabbitMQConfig = Configuration.GetRabbitMQConfiguration("RabbitMQConfig");
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        private readonly RabbitMQConfiguration rabbitMQConfig;

        /// <summary>
        /// Configurates the services.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddSingleton(logger);
            services.AddSingleton<DataServer>();
            services.AddSingleton<IUsersCacheAdapter>(Program.CacheAdapter);
            services.AddSingleton(Program.JwtCacheAdapter);

            services.AddRabbitMQClient(rabbitMQConfig, TimeSpan.FromSeconds(10));

            services.AddHealthChecks().AddCheck<SimpleHealthCheck>("isAlive");

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.WriteIndented = false;
                options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JwtAuth:Issuer"],
                    ValidAudience = Configuration["JwtAuth:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Program.SecretKey),
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new RevokableJwtSecurityTokenHandler(Program.CacheAdapter));
                options.Validate();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth WEB API", Version = "v1" });
                string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

        }

        /// <summary>
        /// Configurates app.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth WEB API v1");
                c.RoutePrefix = string.Empty;
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHealthChecks("/isAlive");



            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    string url = @$"http://localhost:{Program.Port}/auth/ping";
                    UriBuilder builder = new(url);
                    using HttpClientHandler handler = new();
                    using HttpClient httpClient = new(handler);
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;
                    logger.Info("Start server {@data}", response);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
        }
    }

    /// <summary>
    /// The simple health check.
    /// </summary>
    public class SimpleHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Checks that service is alive!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            UriBuilder builder = new()
            {
                Host= "localhost",
                Port = Program.Port,
                Scheme = "http",
                Path = "/auth/ping"
            };
            HttpClient client = new()
            {
                BaseAddress = builder.Uri,
                Timeout = TimeSpan.FromSeconds(1),
            };
            try
            {
                HttpResponseMessage result = await client.GetAsync(client.BaseAddress, cancellationToken);
                bool checkStatusCode = result.IsSuccessStatusCode;
                string stringResult = await result.Content.ReadAsStringAsync(cancellationToken);
                if (!checkStatusCode || string.IsNullOrEmpty(stringResult))
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, "Ping is not sucess!");
                }
                return new HealthCheckResult(HealthStatus.Healthy, stringResult);
            }
            catch(Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Error on posting ping request!", ex);
            }

        }
    }
}