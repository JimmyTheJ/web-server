using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VueServer.Domain;
using VueServer.Domain.Enums;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Weight.Context;
using VueServer.Modules.Weight.Services;

namespace VueServer.Modules.Weight
{
    internal class Startup
    {
        public Startup() { }

        public void Load(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            DatabaseTypes dbType = (DatabaseTypes)config.GetSection("Options").GetValue<int>("DatabaseType");
            if (dbType == DatabaseTypes.MSSQLSERVER)
            {
                services.AddTransient<IWeightContext, SqlServerWeightContext>();
                services.AddEntityFrameworkSqlServer().AddDbContext<IWeightContext, SqlServerWeightContext>
                    (options => options.UseSqlServer(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.SQLITE)
            {
                services.AddTransient<IWeightContext, SqliteWeightContext>();
                services.AddEntityFrameworkSqlite().AddDbContext<IWeightContext, SqliteWeightContext>
                    (options => options.UseSqlite(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.MYSQL)
            {
                services.AddTransient<IWeightContext, MySqlWeightContext>();
                services.AddEntityFrameworkMySql().AddDbContext<IWeightContext, MySqlWeightContext>
                    (options => options.UseMySql(ConnectionStrings.WSCONTEXT, ServerVersion.AutoDetect(ConnectionStrings.WSCONTEXT), a => a.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY)), ServiceLifetime.Scoped);
            }

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
