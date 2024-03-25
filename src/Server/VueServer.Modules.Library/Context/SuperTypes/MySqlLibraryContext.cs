using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Extensions;

namespace VueServer.Modules.Library.Context
{
    public sealed class MySqlLibraryContext : LibraryContext
    {
        public MySqlLibraryContext() : base() { }

        public MySqlLibraryContext(DbContextOptions<MySqlLibraryContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.SetupWSContextModelForSqliteMySql();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(ConnectionStrings.WSCONTEXT, ServerVersion.AutoDetect(ConnectionStrings.WSCONTEXT), b => b.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY));
            }
        }
    }
}
