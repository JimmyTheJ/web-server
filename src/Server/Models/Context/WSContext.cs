using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VueServer.Domain;
using VueServer.Models.Chat;
using VueServer.Models.Directory;
using VueServer.Models.Library;
using VueServer.Models.Modules;
using VueServer.Models.User;

namespace VueServer.Models.Context
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
            #region Library

            modelBuilder.Entity<Book>().HasOne(x => x.Bookcase).WithMany(x => x.Books).HasForeignKey(x => x.BookcaseId).IsRequired(false);
            modelBuilder.Entity<Book>().HasOne(x => x.Series).WithMany(x => x.Books).HasForeignKey(x => x.SeriesId).IsRequired(false);
            modelBuilder.Entity<Book>().HasOne(x => x.Shelf).WithMany(x => x.Books).HasForeignKey(x => x.ShelfId).IsRequired(false);

            // Book to Author many to many setup
            modelBuilder.Entity<BookAuthor>().HasKey(x => new { x.BookId, x.AuthorId });
            modelBuilder.Entity<BookAuthor>().HasOne(x => x.Author).WithMany(x => x.BookAuthors).HasForeignKey(x => x.AuthorId);
            modelBuilder.Entity<BookAuthor>().HasOne(x => x.Book).WithMany(x => x.BookAuthors).HasForeignKey(x => x.BookId);

            // Book to Genre many to many setup
            modelBuilder.Entity<BookGenre>().HasKey(x => new { x.BookId, x.GenreId });
            modelBuilder.Entity<BookGenre>().HasOne(x => x.Genre).WithMany(x => x.BookGenres).HasForeignKey(x => x.GenreId);
            modelBuilder.Entity<BookGenre>().HasOne(x => x.Book).WithMany(x => x.BookGenres).HasForeignKey(x => x.BookId);

            #endregion

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

            #region Conversation

            // Conversation User many to many setup
            modelBuilder.Entity<ConversationHasUser>().HasKey(x => new { x.ConversationId, x.UserId });
            modelBuilder.Entity<ConversationHasUser>().HasOne(x => x.Conversation).WithMany(x => x.ConversationUsers).HasForeignKey(x => x.ConversationId);
            modelBuilder.Entity<ConversationHasUser>().HasOne(x => x.User).WithMany(x => x.ConversationUsers).HasForeignKey(x => x.UserId);

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
            SeedGenres(modelBuilder);
            SeedModules(modelBuilder);
            SeedModuleFeatures(modelBuilder);
            SeedWSSettings(modelBuilder);
        }

        #region -> Database tables

        #region -> Misc

        public DbSet<ServerSettings> ServerSettings { get; set; }

        public DbSet<Notes> Notes { get; set; }

        public DbSet<Weight> Weight { get; set; }

        #endregion

        #region -> Chat System

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationHasUser> ConversationHasUser { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }
        public DbSet<ReadReceipt> ReadReceipts { get; set; }

        #endregion

        #region -> Directory

        public DbSet<ServerUserDirectory> ServerUserDirectory { get; set; }
        public DbSet<ServerGroupDirectory> ServerGroupDirectory { get; set; }

        #endregion

        #region -> Modules

        public DbSet<ModuleAddOn> Modules { get; set; }
        public DbSet<UserHasModuleAddOn> UserHasModule { get; set; }
        public DbSet<ModuleFeature> Features { get; set; }
        public DbSet<UserHasModuleFeature> UserHasFeature { get; set; }

        #endregion

        #region -> Library 

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Bookcase> Bookcases { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<BookAuthor> BookHasAuthors { get; set; }
        public DbSet<BookGenre> BookHasGenres { get; set; }

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
        }

        private void SeedGenres(ModelBuilder modelBuilder)
        {
            int id = 1;
            // Non-Fiction
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Art" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Autobiography" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Biography" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Book review" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Cookbook" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Diary" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Dictionary" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Encyclopedia" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Guide" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Health" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "History" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Journal" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Math" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Memoir" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Prayer" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Religion, spirituality, and new age" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Textbook" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Review" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Science" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Self help" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "Travel" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = false, Name = "True crime" });

            // Fiction
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Action and adventure" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Alternative history" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Anthology" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Chick lit" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Children's" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Comic book" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Coming-of-age" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Crime" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Drama" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Fairytale" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Fantasy" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Graphic novel" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Historical fiction" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Horror" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Mystery" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Paranormal romance" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Picture book" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Poetry" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Political thriller" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Romance" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Satire" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Science fiction" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Short story" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Suspense" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Thriller" });
            modelBuilder.Entity<Genre>().HasData(new Genre { Id = id++, Fiction = true, Name = "Young adult" });
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
