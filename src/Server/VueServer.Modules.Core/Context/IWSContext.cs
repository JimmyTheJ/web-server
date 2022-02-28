using Microsoft.EntityFrameworkCore;
using VueServer.Core;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Core.Models.Modules;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Context
{
    public interface IWSContext : IBaseContext
    {
        #region -> Misc

        DbSet<ServerSettings> ServerSettings { get; set; }

        DbSet<Notes> Notes { get; set; }

        #endregion

        #region -> Modules

        DbSet<ModuleAddOn> Modules { get; set; }
        DbSet<UserHasModuleAddOn> UserHasModule { get; set; }
        DbSet<ModuleFeature> Features { get; set; }
        DbSet<UserHasModuleFeature> UserHasFeature { get; set; }

        #endregion

        #region -> Identity

        DbSet<WSUser> Users { get; set; }
        DbSet<WSRole> Roles { get; set; }
        DbSet<WSUserInRoles> UserRoles { get; set; }
        DbSet<WSUserLogin> UserLogin { get; set; }
        DbSet<WSUserTokens> UserTokens { get; set; }

        DbSet<WSUserProfile> UserProfile { get; set; }
        DbSet<WSGuestLogin> GuestLogin { get; set; }

        #endregion
    }
}
