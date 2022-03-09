using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Directory.Models;

namespace VueServer.Modules.Directory.Context
{
    public class DirectoryContext : WSContext, IDirectoryContext
    {
        public DirectoryContext() : base() { }
        public DirectoryContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            SeedWSSettings(modelBuilder);
        }

        public DbSet<ServerUserDirectory> ServerUserDirectory { get; set; }
        public DbSet<ServerGroupDirectory> ServerGroupDirectory { get; set; }

        private void SeedWSSettings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerSettings>().HasData(new ServerSettings() { Key = DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.ShouldUseDefaultPath, Value = "0" });
            modelBuilder.Entity<ServerSettings>().HasData(new ServerSettings() { Key = DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.DefaultPathValue, Value = "" });
        }
    }
}
