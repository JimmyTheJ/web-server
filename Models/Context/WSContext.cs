using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using System;
using System.Threading.Tasks;
using VueServer.Models.User;

namespace VueServer.Models.Context
{
    public class WSContext : DbContext, IWSContext
    {
        private string _serverString;

        private readonly SqliteConnection Connection;

        public WSContext ()
        {

        }

        public WSContext(SqliteConnection conn)
        {
            Connection = conn;
        }

        public WSContext(DbContextOptions<WSContext> options) : base(options)
        {
            _serverString = (options.FindExtension<SqlServerOptionsExtension>()).ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Console.WriteLine($"WSContext: On Configuring. With connection string: {_serverString}");
            if (Connection == null)
            {
                optionsBuilder.UseSqlServer(_serverString, a => a.MigrationsAssembly("VueServer"));
            }
            else
            {
                optionsBuilder.UseSqlServer(Connection, a => a.MigrationsAssembly("VueServer"));
            }            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }

        #region -> Database tables

        public DbSet<Notes> Notes { get; set; }

        public DbSet<Weight> Weight { get; set; }

        #region -> Identity

        public DbSet<WSUser> Users { get; set; }
        public DbSet<WSRole> Roles { get; set; }
        public DbSet<WSUserInRoles> UserRoles { get; set; }
        public DbSet<WSUserLogin> UserLogin { get; set; }
        public DbSet<WSUserTokens> UserTokens { get; set; }

        #endregion

        #endregion
    }
}
