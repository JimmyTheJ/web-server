using Microsoft.EntityFrameworkCore;
using VueServer.Domain;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Core.Models.Modules;
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

            // Seeding
            SeedWSSettings(modelBuilder);
            SeedModule(modelBuilder);
        }

        public DbSet<ServerUserDirectory> ServerUserDirectory { get; set; }
        public DbSet<ServerGroupDirectory> ServerGroupDirectory { get; set; }

        private void SeedWSSettings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerSettings>().HasData(new ServerSettings() { Key = DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.ShouldUseDefaultPath, Value = "0" });
            modelBuilder.Entity<ServerSettings>().HasData(new ServerSettings() { Key = DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.DefaultPathValue, Value = "" });
        }

        private void SeedModule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DirectoryConstants.ModuleAddOn.Id, Name = DirectoryConstants.ModuleAddOn.Name });

            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DirectoryConstants.ModuleFeatures.DELETE_ID, Name = DirectoryConstants.ModuleFeatures.DELETE_NAME, ModuleAddOnId = DirectoryConstants.ModuleAddOn.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DirectoryConstants.ModuleFeatures.UPLOAD_ID, Name = DirectoryConstants.ModuleFeatures.UPLOAD_NAME, ModuleAddOnId = DirectoryConstants.ModuleAddOn.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DirectoryConstants.ModuleFeatures.VIEWER_ID, Name = DirectoryConstants.ModuleFeatures.VIEWER_NAME, ModuleAddOnId = DirectoryConstants.ModuleAddOn.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DirectoryConstants.ModuleFeatures.CREATE_ID, Name = DirectoryConstants.ModuleFeatures.CREATE_NAME, ModuleAddOnId = DirectoryConstants.ModuleAddOn.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DirectoryConstants.ModuleFeatures.MOVE_ID, Name = DirectoryConstants.ModuleFeatures.MOVE_NAME, ModuleAddOnId = DirectoryConstants.ModuleAddOn.Id });
        }
    }
}
