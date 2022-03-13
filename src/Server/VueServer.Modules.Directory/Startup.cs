using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VueServer.Domain;
using VueServer.Domain.Enums;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Directory.Context;
using VueServer.Modules.Directory.Services;

namespace VueServer.Modules.Directory
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            DatabaseTypes dbType = (DatabaseTypes)config.GetSection("Options").GetValue<int>("DatabaseType");
            if (dbType == DatabaseTypes.MSSQLSERVER)
            {
                services.AddTransient<IDirectoryContext, SqlServerDirectoryContext>();
                services.AddEntityFrameworkSqlServer().AddDbContext<IDirectoryContext, SqlServerDirectoryContext>
                    (options => options.UseSqlServer(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.SQLITE)
            {
                services.AddTransient<IDirectoryContext, SqliteDirectoryContext>();
                services.AddEntityFrameworkSqlite().AddDbContext<IDirectoryContext, SqliteDirectoryContext>
                    (options => options.UseSqlite(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.MYSQL)
            {
                services.AddTransient<IDirectoryContext, MySqlDirectoryContext>();
                services.AddEntityFrameworkMySql().AddDbContext<IDirectoryContext, MySqlDirectoryContext>
                    (options => options.UseMySql(ConnectionStrings.WSCONTEXT, ServerVersion.AutoDetect(ConnectionStrings.WSCONTEXT), a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }

            services.AddScoped<IDirectoryService, DirectoryService>();
            services.AddScoped<IDirectoryAdminService, DirectoryAdminService>();
        }

        public void Create(IApplicationBuilder app)
        {
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
