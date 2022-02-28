using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using VueServer.Modules.Weight.Services;

namespace VueServer.Modules.Weight
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services)
        {
            services.AddScoped<IWeightService, WeightService>();
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
