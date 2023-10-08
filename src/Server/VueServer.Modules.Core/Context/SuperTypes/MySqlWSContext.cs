using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Context
{
    public sealed class MySqlWSContext : WSContext
    {
        public MySqlWSContext() : base() { }

        public MySqlWSContext(DbContextOptions<MySqlWSContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Remove Cluster Id from this database type
            modelBuilder.Entity<WSUser>().Ignore(x => x.ClusterId);
            modelBuilder.Entity<WSRole>().Ignore(x => x.ClusterId);

            // Setup PK and remove Cluster Id from this database type
            modelBuilder.Entity<WSGuestLogin>().HasKey(x => x.IPAddress);
            modelBuilder.Entity<WSGuestLogin>().Ignore(x => x.ClusterId);
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
