using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Weight.Models;

namespace VueServer.Modules.Weight.Context
{
    public class WeightContext : WSContext, IWeightContext
    {
        public DbSet<Weights> Weight { get; set; }
    }
}
