using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Extensions;

namespace VueServer.Modules.Library.Context
{
    public sealed class SqlServerLibraryContext : LibraryContext
    {
        public SqlServerLibraryContext() : base() { }

        public SqlServerLibraryContext(DbContextOptions<MySqlLibraryContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.SetupWSContextModelForSqlServer();
        }

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
