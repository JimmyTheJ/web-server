using Microsoft.EntityFrameworkCore;

namespace VueServer.Modules.Core.Context
{
    public class MySqlWSContext : WSContext, IWSContext
    {
        public MySqlWSContext() : base() { }

        public MySqlWSContext(DbContextOptions<WSContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(ConnectionStrings.WSCONTEXT, ServerVersion.AutoDetect(ConnectionStrings.WSCONTEXT), b => b.MigrationsAssembly("VueServer"));
            }
        }
    }
}
