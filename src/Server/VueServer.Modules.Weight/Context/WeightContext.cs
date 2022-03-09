using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Weight.Models;

namespace VueServer.Modules.Weight.Context
{
    public class WeightContext : WSContext, IWeightContext
    {
        public WeightContext() : base() { }

        public WeightContext(DbContextOptions options) : base(options) { }

        public DbSet<Weights> Weight { get; set; }
    }
}
