using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VueServer.Core;
using VueServer.Domain;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Core.Models.Modules;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Context
{
    public class WSContext : BaseContext, IWSContext
    {
        private IPasswordHasher<WSUser> _passwordHasher;

        public WSContext()
        {
            Initialize();
        }

        public WSContext(DbContextOptions options) : base(options)
        {
            Initialize();
        }

        private void Initialize()
        {
            _passwordHasher = new PasswordHasher<WSUser>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region ModuleAddOns

            // User Modules many to many setup
            modelBuilder.Entity<UserHasModuleAddOn>().HasKey(x => new { x.UserId, x.ModuleAddOnId });
            modelBuilder.Entity<UserHasModuleAddOn>().HasOne(x => x.User).WithMany(x => x.UserModuleAddOns).HasForeignKey(x => x.UserId);
            modelBuilder.Entity<UserHasModuleAddOn>().HasOne(x => x.ModuleAddOn).WithMany(x => x.UserModuleAddOns).HasForeignKey(x => x.ModuleAddOnId);

            // Module Features many to many setup
            modelBuilder.Entity<UserHasModuleFeature>().HasKey(x => new { x.UserId, x.ModuleFeatureId });
            modelBuilder.Entity<UserHasModuleFeature>().HasOne(x => x.User).WithMany(x => x.UserModuleFeatures).HasForeignKey(x => x.UserId);
            modelBuilder.Entity<UserHasModuleFeature>().HasOne(x => x.ModuleFeature).WithMany(x => x.UserModuleFeatures).HasForeignKey(x => x.ModuleFeatureId);

            #endregion

            // Data Seeding
            SeedIdentity(modelBuilder);
            SeedModules(modelBuilder);
        }

        #region -> Database tables

        #region -> Misc

        public DbSet<ServerSettings> ServerSettings { get; set; }

        public DbSet<Notes> Notes { get; set; }

        #endregion

        #region -> Modules

        public DbSet<ModuleAddOn> Modules { get; set; }
        public DbSet<UserHasModuleAddOn> UserHasModule { get; set; }
        public DbSet<ModuleFeature> Features { get; set; }
        public DbSet<UserHasModuleFeature> UserHasFeature { get; set; }

        #endregion

        #region -> Identity

        public DbSet<WSUser> Users { get; set; }
        public DbSet<WSRole> Roles { get; set; }
        public DbSet<WSUserInRoles> UserRoles { get; set; }
        public DbSet<WSUserLogin> UserLogin { get; set; }
        public DbSet<WSUserTokens> UserTokens { get; set; }
        public DbSet<WSUserProfile> UserProfile { get; set; }
        public DbSet<WSGuestLogin> GuestLogin { get; set; }

        #endregion

        #endregion

        #region -> Private Functions

        private void SeedIdentity(ModelBuilder modelBuilder)
        {
            int i = 0;
            modelBuilder.Entity<WSRole>().HasData(new WSRole
            {
                Id = DomainConstants.Authentication.USER_STRING.ToLower(),
                ClusterId = ++i,
                DisplayName = DomainConstants.Authentication.USER_STRING,
                NormalizedName =
                DomainConstants.Authentication.USER_STRING.ToUpper()
            });
            modelBuilder.Entity<WSRole>().HasData(new WSRole
            {
                Id = DomainConstants.Authentication.ELEVATED_STRING.ToLower(),
                ClusterId = ++i,
                DisplayName = DomainConstants.Authentication.ELEVATED_STRING,
                NormalizedName = DomainConstants.Authentication.ELEVATED_STRING.ToUpper()
            });
            modelBuilder.Entity<WSRole>().HasData(new WSRole
            {
                Id = DomainConstants.Authentication.ADMINISTRATOR_STRING.ToLower(),
                ClusterId = ++i,
                DisplayName = DomainConstants.Authentication.ADMINISTRATOR_STRING,
                NormalizedName = DomainConstants.Authentication.ADMINISTRATOR_STRING.ToUpper()
            });

            SeedAdministrator(modelBuilder);
        }

        private void SeedAdministrator(ModelBuilder modelBuilder)
        {
            var adminUser = new WSUser
            {
                Id = DomainConstants.Authentication.ADMIN_STRING.ToLower(),
                ClusterId = 1L,
                NormalizedUserName = DomainConstants.Authentication.ADMIN_STRING.ToUpper(),
                DisplayName = DomainConstants.Authentication.ADMIN_STRING,
                PasswordExpired = true,
                Active = false
            };

            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, DomainConstants.Authentication.DEFAULT_PASSWORD);
            modelBuilder.Entity<WSUser>().HasData(adminUser);
            modelBuilder.Entity<WSUserInRoles>().HasData(new WSUserInRoles() { Id = 1L, RoleId = DomainConstants.Authentication.ADMINISTRATOR_STRING.ToLower(), UserId = adminUser.Id });
            modelBuilder.Entity<WSUserProfile>().HasData(new WSUserProfile() { Id = 1, AvatarPath = null, UserId = DomainConstants.Authentication.ADMIN_STRING.ToLower() });
        }
        private void SeedModules(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Documentation.Id, Name = DomainConstants.Models.ModuleAddOns.Documentation.Name });
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Notes.Id, Name = DomainConstants.Models.ModuleAddOns.Notes.Name });
        }

        #endregion
    }
}
