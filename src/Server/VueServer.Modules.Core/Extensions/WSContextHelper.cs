using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void SetupWSContextModelForSqliteMySql(this ModelBuilder modelBuilder)
        {
            // Remove Cluster Id from this database type
            modelBuilder.Entity<WSUser>().Ignore(x => x.ClusterId);
            modelBuilder.Entity<WSRole>().Ignore(x => x.ClusterId);

            // Setup PK and remove Cluster Id from this database type
            modelBuilder.Entity<WSGuestLogin>().HasKey(x => x.IPAddress);
            modelBuilder.Entity<WSGuestLogin>().Ignore(x => x.ClusterId);
        }

        public static void SetupWSContextModelForSqlServer(this ModelBuilder modelBuilder)
        {
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
    }
}
