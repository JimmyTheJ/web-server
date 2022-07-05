using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using VueServer.Domain;
using VueServer.Domain.Enums;
using VueServer.Modules.Chat.Context;
using VueServer.Modules.Chat.Services;
using VueServer.Modules.Chat.Services.Hubs;
using VueServer.Modules.Core;
using VueServer.Modules.Core.Context;

namespace VueServer.Modules.Chat
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
            }).AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new LowercaseContractResolver();
                options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            DatabaseTypes dbType = (DatabaseTypes)config.GetSection("Options").GetValue<int>("DatabaseType");
            if (dbType == DatabaseTypes.MSSQLSERVER)
            {
                services.AddTransient<IChatContext, SqlServerChatContext>();
                services.AddEntityFrameworkSqlServer().AddDbContext<IChatContext, SqlServerChatContext>
                    (options => options.UseSqlServer(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.SQLITE)
            {
                services.AddTransient<IChatContext, SqliteChatContext>();
                services.AddEntityFrameworkSqlite().AddDbContext<IChatContext, SqliteChatContext>
                    (options => options.UseSqlite(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.MYSQL)
            {
                services.AddTransient<IChatContext, MySqlChatContext>();
                services.AddEntityFrameworkMySql().AddDbContext<IChatContext, MySqlChatContext>
                    (options => options.UseMySql(ConnectionStrings.WSCONTEXT, ServerVersion.AutoDetect(ConnectionStrings.WSCONTEXT), a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }

            services.AddScoped<IChatService, ChatService>();
        }

        public void Create(IApplicationBuilder app)
        {

        }

        public void SetCustomEndpoints(IEndpointRouteBuilder endpoint)
        {
            endpoint.MapHub<ChatHub>("/chat-hub");
        }
    }
}
