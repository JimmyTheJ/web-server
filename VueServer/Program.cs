using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;

namespace VueServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(serverOptions =>
                    {

                    })
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .ConfigureLogging(log =>
                    {
                        log.AddConsole();
                    });
                })
                .Build();
                
            host.Run();
        }
    }
}
