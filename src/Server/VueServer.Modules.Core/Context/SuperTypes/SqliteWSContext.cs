using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Context
{
    public sealed class SqliteWSContext : WSContext
    {
        private readonly SqliteConnection _connection;

        public SqliteWSContext() : base() { }

        public SqliteWSContext(SqliteConnection connection) : base()
        {
            _connection = connection;
        }

        public SqliteWSContext(DbContextOptions<SqliteWSContext> options) : base(options) { }

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
            if (_connection != null)
            {
                optionsBuilder.UseSqlite(_connection, b => b.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY));
            }
            else if (!optionsBuilder.IsConfigured)
            {
                base.OnConfiguring(optionsBuilder); // TODO: Determine if this is needed
                optionsBuilder.UseSqlite(ConnectionStrings.WSCONTEXT, b => b.MigrationsAssembly(DomainConstants.MIGRATION_ASSEMBLY));
            }
        }
    }
}
