using Microsoft.EntityFrameworkCore;

namespace VueServer.Modules.Core.Context
{
    public class SqlServerWSContext : WSContext, IWSContext
    {
        public SqlServerWSContext() : base() { }

        public SqlServerWSContext(DbContextOptions<WSContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionStrings.WSCONTEXT, b => b.MigrationsAssembly("VueServer"));
            }
        }
    }
}
