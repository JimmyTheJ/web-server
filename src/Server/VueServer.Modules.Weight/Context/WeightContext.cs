using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models.Modules;
using VueServer.Modules.Weight.Models;

namespace VueServer.Modules.Weight.Context
{
    public class WeightContext : WSContext, IWeightContext
    {
        public WeightContext() : base() { }

        public WeightContext(DbContextOptions options) : base(options) { }

        public DbSet<Weights> Weight { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Data Seeding
            SeedModule(modelBuilder);
        }

        private void SeedModule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = WeightConstants.ModuleAddOn.Id, Name = WeightConstants.ModuleAddOn.Name });
        }
    }
}
