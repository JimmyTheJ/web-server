using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VueServer.Domain.Interface;
using Xunit;

namespace VueServer.Test.Integration.Controller
{
    public class AccountControllerTest : IClassFixture<Server<TestStartup>>
    {
        private readonly Mock<IWebHostEnvironment> Environment = new Mock<IWebHostEnvironment>();

        private Server<TestStartup> VueServer;

        private const string Token = "3836-573-254gzds6-34fdtfg-65-7244d";
        private const string RefreshToken = "d-av45t-fhgh-71uhgf-42lrf-r6d5-gf";
        private const string USERNAME = "username";
        private const string PASSWORD = "password";
        private const string ROLE = "Administrator";

        public AccountControllerTest(Server<TestStartup> server)
        {
            VueServer = server;

            SetupMocks();
        }

        private void SetupMocks()
        {
            VueServer.CodeFactory.Setup(codeFactory => codeFactory.GetStatusCode(It.IsAny<IServerResult>())).Returns(new OkResult());
        }

        //[Fact]
        //public async Task TestSignUp ()
        //{
        //    RegisterRequest registerRequest = new RegisterRequest(USERNAME, PASSWORD, PASSWORD, ROLE);

        //    HttpResponseMessage response = await VueServer.Client.PostAsJsonAsync("/api/account/register", new JsonContent(registerRequest));

        //    VueServer.AccountService.Verify(o => o.Register(It.IsAny<RegisterRequest>()));
        //}

        //[Fact]
        //public async Task TestLogin ()
        //{
        //    LoginRequest loginRequest = new LoginRequest(USERNAME, PASSWORD);

        //    HttpResponseMessage response = await VueServer.Client.PostAsJsonAsync("/api/account/login", new JsonContent(loginRequest));

        //    VueServer.AccountService.Verify(o => o.Login(It.IsAny<LoginRequest>()));
        //}
    }
}
