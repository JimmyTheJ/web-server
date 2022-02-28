using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Directory.Models;

namespace VueServer.Modules.Directory.Context
{
    public class DirectoryContext : WSContext, IDirectoryContext
    {
        public DbSet<ServerUserDirectory> ServerUserDirectory { get; set; }
        public DbSet<ServerGroupDirectory> ServerGroupDirectory { get; set; }
    }
}
