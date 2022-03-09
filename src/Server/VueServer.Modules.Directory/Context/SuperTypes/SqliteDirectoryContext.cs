using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Context;

namespace VueServer.Modules.Directory.Context
{
    public sealed class SqliteDirectoryContext : DirectoryContext
    {
        private readonly SqliteConnection _connection;

        public SqliteDirectoryContext() : base() { }

        public SqliteDirectoryContext(SqliteConnection connection) : base()
        {
            _connection = connection;
        }

        public SqliteDirectoryContext(DbContextOptions<MySqlDirectoryContext> options) : base(options) { }

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
