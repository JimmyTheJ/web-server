using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using VueServer.Modules.Directory.Services;

namespace VueServer.Modules.Directory
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services)
        {
            services.AddScoped<IDirectoryService, DirectoryService>();
        }

        public void Create(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
