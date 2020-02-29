using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using VueServer.Models.User;

namespace VueServer.Models.Context
{
    public class SqliteWSContext : WSContext, IWSContext
    {
        private readonly SqliteConnection _connection;

        public SqliteWSContext() { }

        public SqliteWSContext(SqliteConnection connection) 
        {
            _connection = connection;
        }

        public SqliteWSContext(DbContextOptions<WSContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_connection != null)
            {
                optionsBuilder.UseSqlite(_connection, b => b.MigrationsAssembly("Astra"));
            }
            else if (!optionsBuilder.IsConfigured)
            {
                base.OnConfiguring(optionsBuilder); // TODO: Determine if this is needed
                optionsBuilder.UseSqlite(ConnectionStrings.WSCONTEXT, b => b.MigrationsAssembly("VueServer"));
            }
        }
    }
}
