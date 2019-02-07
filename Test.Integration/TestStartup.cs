using Moq;
using System;
using Xunit;
using VueServer.Services.Interface;
using VueServer.Models.Context;
using Microsoft.AspNetCore.Hosting;
using Castle.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Antiforgery;

namespace VueServer.Test.Integration
{
    public class TestStartup : Startup {

        public TestStartup(IWebHostEnvironment environment) : base(environment) { }
    }
}
