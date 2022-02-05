using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.IO;
using System.Net.Http;
using VueServer.Core.Status;
using VueServer.Models.Context;
using VueServer.Services.Account;

namespace VueServer.Test.Integration
{
    public class Server<TestStartup> : IDisposable where TestStartup : class
    {
        private readonly TestServer VueServer;

        public Mock<IAccountService> AccountService = new Mock<IAccountService>();
        public Mock<IWSContext> VueContext = new Mock<IWSContext>();
        public Mock<IStatusCodeFactory<IActionResult>> CodeFactory = new Mock<IStatusCodeFactory<IActionResult>>();

        private const string DOMAIN = "localhost";
        private const int PORT = 7757;

        public Server()
        {
            var basepath = Directory.GetCurrentDirectory();
            var path = Path.Combine(basepath, "..", "..", "..", "..", "VueServer");

            IWebHostBuilder webHostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(path)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(serviceProvider => AccountService.Object);
                    services.AddSingleton(serviceProvider => VueContext.Object);
                    services.AddSingleton(serviceProvider => CodeFactory.Object);
                })
                .UseStartup<TestStartup>();

            VueServer = new TestServer(webHostBuilder);

            Client = VueServer.CreateClient();
            Client.BaseAddress = BuildUrl();
        }

        private Uri BuildUrl()
        {
            return new Uri("https://" + DOMAIN + ":" + PORT.ToString());
        }

        public HttpClient Client { get; }

        public void Dispose()
        {
            Client.Dispose();
            VueServer.Dispose();
        }
    }
}
