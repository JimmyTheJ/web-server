using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using System;
using VueServer.Models.Account;

namespace VueServer.Models.Context
{
    public class UserContext : IdentityDbContext<ServerIdentity>
    {
        private readonly string _serverString;

        public UserContext()
        {

        }

        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            _serverString = (options.FindExtension<SqlServerOptionsExtension>()).ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Console.WriteLine($"UserContext: On Configuring. With connection string: {_serverString}");
            optionsBuilder.UseSqlServer(_serverString, a => a.MigrationsAssembly("VueServer"));
        }

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }*/

        public new DbSet<ServerIdentity> Users { get; set; }

        public DbSet<UserTokens> UserWebTokens { get; set; }

    }
}
