using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using VueServer.Modules.Chat.Services.Chat;
using VueServer.Modules.Chat.Services.Hubs;

namespace VueServer.Modules.Chat
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services)
        {
            services.AddScoped<IChatService, ChatService>();
        }

        public void Create(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chat-hub");
                endpoints.MapControllers();
            });
        }
    }
}
