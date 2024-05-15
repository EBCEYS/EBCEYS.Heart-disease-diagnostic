using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Extensions;
using DiagnoseRestService.Server;
using HeartDiseasesDiagnosticExtentions.JsonExtensions;
using HeartDiseasesDiagnosticExtentions.RabbitMQExtensions;
using JWTExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UsersCache;
using DiagnoseRESTService.MQControllers;
using CacheAdapters.UsersCache;
using UsersCache.DiagnoseCache;

namespace DiagnoseRestService
{
    /// <summary>
    /// 
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
            RPCRabbitMQClientConfig = Configuration.GetRabbitMQConfiguration("RPCRabbitMQConfig");
            DBRabbitMQClientConfig = Configuration.GetRabbitMQConfiguration("DBRabbitMQConfig");
            DiagnoseResultsMQServerConfig = Configuration.GetRabbitMQConfiguration("DiagnoseResultMQConfig");
            UsersCacheAdapter = new UsersCacheAdapter(Configuration.GetSection("CacheSettings").Get<CacheAdapterSettings>()!);
            DiagnoseCacheAdapter = new DiagnoseCacheAdapter(Configuration.GetSection("DiagnoseResultsCacheSettings").Get<CacheAdapterSettings>()!);
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        private readonly RabbitMQConfiguration RPCRabbitMQClientConfig;
        private readonly RabbitMQConfiguration DBRabbitMQClientConfig;
        private readonly RabbitMQConfiguration DiagnoseResultsMQServerConfig;
        private readonly IUsersCacheAdapter UsersCacheAdapter;
        private readonly IDiagnoseCacheAdapter DiagnoseCacheAdapter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddSingleton(logger);
            services.AddSingleton<DataServer>(sr =>
            {
                return new(logger
                    , Configuration
                    , new RabbitMQClient(sr.GetService<ILogger<RabbitMQClient>>()!, RPCRabbitMQClientConfig, TimeSpan.FromSeconds(5))
                    , new RabbitMQClient(sr.GetService<ILogger<RabbitMQClient>>()!, DBRabbitMQClientConfig, TimeSpan.FromSeconds(10))
                    , DiagnoseCacheAdapter);
            });

            services.AddRabbitMQController<DiagnoseResultsMQController>();
            services.AddRabbitMQMappedServer(DiagnoseResultsMQServerConfig);

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
                options.SecurityTokenValidators.Add(new RevokableJwtSecurityTokenHandler(UsersCacheAdapter));
                options.Validate();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Heart Diseases Diagnostic WEB API", Version = "v1" });
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
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Heart Diseases Diagnostic WEB API v1"));

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



            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    string url = @$"http://localhost:80/ping";
                    UriBuilder builder = new(url);
                    using HttpClientHandler handler = new();
                    using HttpClient httpClient = new(handler);
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;
                    logger.Debug("Start server {@data}", response);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
        }
    }
}
