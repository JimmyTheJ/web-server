using Microsoft.AspNetCore.Hosting;

namespace VueServer.Test.Integration
{
    public class TestStartup : Startup
    {

        public TestStartup(IWebHostEnvironment environment) : base(environment) { }
    }
}
