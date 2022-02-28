using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Weight.Models;

namespace VueServer.Modules.Weight.Context
{
    public interface IWeightContext : IWSContext
    {
        DbSet<Weights> Weight { get; set; }
    }
}
