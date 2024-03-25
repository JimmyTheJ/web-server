using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Extensions;

namespace VueServer.Modules.Chat.Context
{
    public sealed class MySqlChatContext : ChatContext
    {
        public MySqlChatContext() : base() { }

        public MySqlChatContext(DbContextOptions<MySqlChatContext> options) : base(options) { }

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
