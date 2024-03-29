﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using VueServer.Modules.Core.Services.User;
using VueServer.Modules.Directory.Context;
using VueServer.Modules.Directory.Services;
using Xunit;
using static VueServer.Domain.DomainConstants.Authentication;

namespace VueServer.Test.Integration.Services
{
    public class DirectoryServiceTest
    {
        private readonly HttpContext HttpContext;

        private readonly Mock<ILoggerFactory> LoggerFactory;

        private readonly Mock<ILogger<DirectoryService>> Logger;

        private readonly Mock<IUserService> User;

        private readonly Mock<IWebHostEnvironment> Env;

        private readonly IConfigurationRoot Config;

        public IDirectoryContext Context { get; set; }

        private const string MAIN_IP = "https://127.0.0.1";
        private const string USERNAME = "Jimmy";

        public DirectoryServiceTest()
        {
            var basepath = Directory.GetCurrentDirectory();

            HttpContext = new TestHttpContext();
            LoggerFactory = new Mock<ILoggerFactory>();
            Logger = LoggerUtilities.LoggerMock<DirectoryService>();
            User = new Mock<IUserService>();
            Env = new Mock<IWebHostEnvironment>();

            Env.Setup(o => o.EnvironmentName).Returns("PRODUCTION");
            Env.Setup(o => o.ContentRootPath).Returns(
                Path.Combine(basepath, "..", "..", ".."));


            var builder = new ConfigurationBuilder()
                .SetBasePath(Env.Object.ContentRootPath)
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Config = builder.Build();

            LoggerFactory.Setup(o => o.CreateLogger(It.IsAny<string>())).Returns(Logger.Object);
        }

        [Fact]
        public void GetAdminDirectoriesNoLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ADMINISTRATOR_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("C", o.Name),
                o => Assert.Equal("D", o.Name)
            );
        }

        [Fact]
        public void GetAdminDirectoriesAdminLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ADMINISTRATOR_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("C", o.Name),
                o => Assert.Equal("D", o.Name)
            );
        }

        [Fact]
        public void GetAdminDirectoriesElevatedLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ADMINISTRATOR_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Downloads", o.Name),
                o => Assert.Equal("Music", o.Name),
                o => Assert.Equal("Movies", o.Name)
            );
        }

        [Fact]
        public void GetAdminDirectoriesGeneralLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ADMINISTRATOR_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Shared Files", o.Name),
                o => Assert.Equal("Test", o.Name)
            );
        }




        [Fact]
        public void GetElevatedDirectoriesNoLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ELEVATED_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Downloads", o.Name),
                o => Assert.Equal("Music", o.Name),
                o => Assert.Equal("Movies", o.Name)
            );
        }

        [Fact]
        public void GetElevatedDirectoriesAdminLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ELEVATED_STRING);
            var dirs = service.GetDirectories();

            Logger.VerifyLog(LogLevel.Warning, "Directory.GetDirectories: Permission escalation attack attempted. Setting level to the lowest setting.");
            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Shared Files", o.Name),
                o => Assert.Equal("Test", o.Name)
            );
        }

        [Fact]
        public void GetElevatedDirectoriesElevatedLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ELEVATED_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Downloads", o.Name),
                o => Assert.Equal("Music", o.Name),
                o => Assert.Equal("Movies", o.Name)
            );
        }

        [Fact]
        public void GetElevatedDirectoriesUserLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, ELEVATED_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Shared Files", o.Name),
                o => Assert.Equal("Test", o.Name)
            );
        }





        [Fact]
        public void GetUserDirectoriesNoLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, USER_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Shared Files", o.Name),
                o => Assert.Equal("Test", o.Name)
            );
        }

        [Fact]
        public void GetUserDirectoriesAdminLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, USER_STRING);
            var dirs = service.GetDirectories();

            Logger.VerifyLog(LogLevel.Warning, "Directory.GetDirectories: Permission escalation attack attempted. Setting level to the lowest setting.");
            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Shared Files", o.Name),
                o => Assert.Equal("Test", o.Name)
            );
        }

        [Fact]
        public void GetUserDirectoriesElevatedLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, USER_STRING);
            var dirs = service.GetDirectories();

            Logger.VerifyLog(LogLevel.Warning, "Directory.GetDirectories: Permission escalation attack attempted. Setting level to the lowest setting.");
            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Shared Files", o.Name),
                o => Assert.Equal("Test", o.Name)
            );
        }

        [Fact]
        public void GetUserDirectoriesGeneralLevel()
        {
            var service = GetDirectoryService(MAIN_IP, USERNAME, USER_STRING);
            var dirs = service.GetDirectories();

            Assert.True(dirs.Result.Code == Domain.Enums.StatusCode.OK);
            Assert.NotNull(dirs.Result.Obj);
            Assert.Collection(dirs.Result.Obj,
                o => Assert.Equal("Shared Files", o.Name),
                o => Assert.Equal("Test", o.Name)
            );
        }




        private DirectoryService GetDirectoryService(string IP, string username, string role)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username.ToLower()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            HttpContext.User = claimsPrincipal;

            User.Setup(o => o.IP).Returns(IP);
            User.Setup(o => o.Id).Returns(username);
            User.Setup(o => o.Context).Returns(HttpContext);

            var service = new DirectoryService(
                LoggerFactory.Object,
                User.Object,
                Env.Object,
                Config,
                Context);

            return service;
        }
    }
}
