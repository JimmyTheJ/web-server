using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using VueServer.Modules.Library.Services;

namespace VueServer.Modules.Library
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services)
        {
            services.AddScoped<ILibraryService, LibraryService>();
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
