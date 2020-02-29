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
    public class WSContext : DbContext, IWSContext
    {
        public WSContext() { }

        public WSContext(DbContextOptions<WSContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }

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
