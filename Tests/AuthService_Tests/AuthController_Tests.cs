using AuthService.Controllers;
using AuthService.Server;
using HeartDiseasesDiagnosticExtentions.AuthExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NLog;

namespace AuthService_Tests
{
    [TestClass]
    public class AuthController_Tests
    {
        private AuthController? controller;
        private readonly LoginModel loginModel = new()
        {
            UserName = "usr",
            Password = "pas"
        };
        private readonly RegisterModel registerModel = new()
        {
            UserName = "reg",
            Password = "pas",
            Role = new[] { "testreg" }
        };
        [TestInitialize]
        public void Initialize()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", false);
            IConfiguration config = builder.Build();
            Mock<DataServer> mock = new(LogManager.CreateNullLogger(), config, null, null);

            mock.Setup(x => x.LoginAsync(loginModel)).ReturnsAsync(() =>
            {
                return new()
                {
                    UserName = loginModel.UserName,
                    ResponseResult = AuthService.ApiResponses.UsersResponseResult.OK,
                    Token = "123",
                    UserId = "123",
                    Role = new[] { "test" }
                };
            });
            mock.Setup(x => x.RegisterAsync(registerModel)).ReturnsAsync(() =>
            {
                return new()
                {
                    ResponseResult = AuthService.ApiResponses.UsersResponseResult.OK,
                    UserId = "reg123",
                    Role = registerModel.Role,
                    UserName = registerModel.UserName
                };
            });
            controller = new(LogManager.CreateNullLogger(), mock.Object);
        }
        [TestMethod]
        public void Ping_Test()
        {

        }

        private T? GetValueFromActionResult<T>(ActionResult<T> actionResult) where T : class
        {
            return (actionResult.Result as OkObjectResult)?.Value as T;
        }
    }
}