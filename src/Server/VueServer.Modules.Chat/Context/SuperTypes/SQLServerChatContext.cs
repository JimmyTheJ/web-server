using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Extensions;

namespace VueServer.Modules.Chat.Context
{
    public sealed class SqlServerChatContext : ChatContext
    {
        public SqlServerChatContext() : base() { }

        public SqlServerChatContext(DbContextOptions<MySqlChatContext> options) : base(options) { }

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
