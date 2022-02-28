using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VueServer.Domain;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Core.Models.Modules;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Context
{
    public class WSContext : DbContext, IWSContext
    {
        private IDbContextTransaction _transaction;
        private IPasswordHasher<WSUser> _passwordHasher;

        public WSContext()
        {
            Initialize();
        }

        public WSContext(DbContextOptions<WSContext> options) : base(options)
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

            #region Identity

            // Setup ClusteredId and Primary key as the Id
            modelBuilder.Entity<WSUser>().HasKey(x => x.Id).IsClustered(false);
            modelBuilder.Entity<WSUser>().HasIndex(x => x.ClusterId).IsUnique().IsClustered();
            modelBuilder.Entity<WSUser>().Property(x => x.ClusterId).ValueGeneratedOnAdd();
            modelBuilder.Entity<WSRole>().HasKey(x => x.Id).IsClustered(false);
            modelBuilder.Entity<WSRole>().HasIndex(x => x.ClusterId).IsUnique().IsClustered();
            modelBuilder.Entity<WSRole>().Property(x => x.ClusterId).ValueGeneratedOnAdd();

            // Setup ClusteredId and Primary key as the IP Address for guest login meta data table
            modelBuilder.Entity<WSGuestLogin>().HasKey(x => x.IPAddress).IsClustered(false);
            modelBuilder.Entity<WSGuestLogin>().HasIndex(x => x.ClusterId).IsUnique().IsClustered();
            modelBuilder.Entity<WSGuestLogin>().Property(x => x.ClusterId).ValueGeneratedOnAdd();

            #endregion

            // Data Seeding
            SeedIdentity(modelBuilder);
            SeedModules(modelBuilder);
            SeedModuleFeatures(modelBuilder);
            SeedWSSettings(modelBuilder);
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

        #region -> Public Functions

        public void BeginTransaction()
        {
            _transaction = Database.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                SaveChanges();
                _transaction.Commit();
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }

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
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Browser.Id, Name = DomainConstants.Models.ModuleAddOns.Browser.Name });
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Chat.Id, Name = DomainConstants.Models.ModuleAddOns.Chat.Name });
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Documentation.Id, Name = DomainConstants.Models.ModuleAddOns.Documentation.Name });
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Library.Id, Name = DomainConstants.Models.ModuleAddOns.Library.Name });
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Notes.Id, Name = DomainConstants.Models.ModuleAddOns.Notes.Name });
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = DomainConstants.Models.ModuleAddOns.Weight.Id, Name = DomainConstants.Models.ModuleAddOns.Weight.Name });
        }

        private void SeedModuleFeatures(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DomainConstants.Models.ModuleFeatures.Browser.DELETE_ID, Name = DomainConstants.Models.ModuleFeatures.Browser.DELETE_NAME, ModuleAddOnId = DomainConstants.Models.ModuleAddOns.Browser.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DomainConstants.Models.ModuleFeatures.Browser.UPLOAD_ID, Name = DomainConstants.Models.ModuleFeatures.Browser.UPLOAD_NAME, ModuleAddOnId = DomainConstants.Models.ModuleAddOns.Browser.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DomainConstants.Models.ModuleFeatures.Browser.VIEWER_ID, Name = DomainConstants.Models.ModuleFeatures.Browser.VIEWER_NAME, ModuleAddOnId = DomainConstants.Models.ModuleAddOns.Browser.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DomainConstants.Models.ModuleFeatures.Browser.CREATE_ID, Name = DomainConstants.Models.ModuleFeatures.Browser.CREATE_NAME, ModuleAddOnId = DomainConstants.Models.ModuleAddOns.Browser.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DomainConstants.Models.ModuleFeatures.Browser.MOVE_ID, Name = DomainConstants.Models.ModuleFeatures.Browser.MOVE_NAME, ModuleAddOnId = DomainConstants.Models.ModuleAddOns.Browser.Id });

            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DomainConstants.Models.ModuleFeatures.Chat.DELETE_MESSAGE_ID, Name = DomainConstants.Models.ModuleFeatures.Chat.DELETE_MESSAGE_NAME, ModuleAddOnId = DomainConstants.Models.ModuleAddOns.Chat.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = DomainConstants.Models.ModuleFeatures.Chat.DELETE_CONVERSATION_ID, Name = DomainConstants.Models.ModuleFeatures.Chat.DELETE_CONVERSATION_NAME, ModuleAddOnId = DomainConstants.Models.ModuleAddOns.Chat.Id });
        }

        private void SeedWSSettings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerSettings>().HasData(new ServerSettings() { Key = DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.ShouldUseDefaultPath, Value = "0" });
            modelBuilder.Entity<ServerSettings>().HasData(new ServerSettings() { Key = DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.DefaultPathValue, Value = "" });
        }

        #endregion
    }
}
