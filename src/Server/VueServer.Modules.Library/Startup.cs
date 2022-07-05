using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VueServer.Domain;
using VueServer.Domain.Enums;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Library.Context;
using VueServer.Modules.Library.Services;

namespace VueServer.Modules.Library
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            DatabaseTypes dbType = (DatabaseTypes)config.GetSection("Options").GetValue<int>("DatabaseType");
            if (dbType == DatabaseTypes.MSSQLSERVER)
            {
                services.AddTransient<ILibraryContext, SqlServerLibraryContext>();
                services.AddEntityFrameworkSqlServer().AddDbContext<ILibraryContext, SqlServerLibraryContext>
                    (options => options.UseSqlServer(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.SQLITE)
            {
                services.AddTransient<ILibraryContext, SqliteLibraryContext>();
                services.AddEntityFrameworkSqlite().AddDbContext<ILibraryContext, SqliteLibraryContext>
                    (options => options.UseSqlite(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.MYSQL)
            {
                services.AddTransient<ILibraryContext, MySqlLibraryContext>();
                services.AddEntityFrameworkMySql().AddDbContext<ILibraryContext, MySqlLibraryContext>
                    (options => options.UseMySql(ConnectionStrings.WSCONTEXT, ServerVersion.AutoDetect(ConnectionStrings.WSCONTEXT), a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }

            services.AddScoped<ILibraryService, LibraryService>();
        }

        public void Create(IApplicationBuilder app)
        {

        }
    }
}
