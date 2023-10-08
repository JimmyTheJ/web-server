using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Context
{
    public sealed class SqlServerWSContext : WSContext
    {
        public SqlServerWSContext() : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Setup ClusteredId and Primary key as the Id
            modelBuilder.Entity<WSUser>().HasKey(x => x.Id).IsClustered(false);
            modelBuilder.Entity<WSUser>().HasIndex(x => x.ClusterId).IsUnique().IsClustered();
            modelBuilder.Entity<WSUser>().Property(x => x.ClusterId).ValueGeneratedOnAdd();
            modelBuilder.Entity<WSRole>().HasKey(x => x.Id).IsClustered(false);
            modelBuilder.Entity<WSRole>().HasIndex(x => x.ClusterId).IsUnique().IsClustered();
            modelBuilder.Entity<WSRole>().Property(x => x.ClusterId).ValueGeneratedOnAdd();

            // Setup ClusteredId and Primary key as the IP Address for guest login meta data table
            modelBuilder.Entity<WSGuestLogin>().HasKey(x => x.IPAddress).IsClustered(false);
            modelBuilder.Entity<WSGuestLogin>().HasIndex(x => x.ClusterId).IsUnique().IsClustered();
            modelBuilder.Entity<WSGuestLogin>().Property(x => x.ClusterId).ValueGeneratedOnAdd();
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
