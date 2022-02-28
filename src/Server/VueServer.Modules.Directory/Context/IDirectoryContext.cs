using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Directory.Models;

namespace VueServer.Modules.Directory.Context
{
    public interface IDirectoryContext : IWSContext
    {
        DbSet<ServerUserDirectory> ServerUserDirectory { get; set; }
        DbSet<ServerGroupDirectory> ServerGroupDirectory { get; set; }
    }
}
