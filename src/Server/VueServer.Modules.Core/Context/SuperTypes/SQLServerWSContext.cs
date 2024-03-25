using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Extensions;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Context
{
    public sealed class SqlServerWSContext : WSContext
    {
        public SqlServerWSContext() : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.SetupWSContextModelForSqlServer();
        }

        public SqlServerWSContext(DbContextOptions<SqlServerWSContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionStrings.WSCONTEXT, b => b.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY));
            }
        }
    }
}
