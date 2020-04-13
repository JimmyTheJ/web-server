using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using VueServer.Models.Library;
using VueServer.Models.Modules;
using VueServer.Models.User;

namespace VueServer.Models.Context
{
    public class WSContext : DbContext, IWSContext
    {
        public WSContext() { }

        public WSContext(DbContextOptions<WSContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<Book>().HasOne(x => x.Genre).WithMany(x => x.Books).HasForeignKey(x => x.GenreId).IsRequired(false);
            modelBuilder.Entity<Book>().HasOne(x => x.Bookshelf).WithMany(x => x.Books).HasForeignKey(x => x.BookshelfId).IsRequired(false);
            modelBuilder.Entity<Book>().HasOne(x => x.Series).WithMany(x => x.Books).HasForeignKey(x => x.SeriesId).IsRequired(false);

            // Book to Author many to many setup
            modelBuilder.Entity<BookAuthor>().HasKey(x => new { x.BookId, x.AuthorId });
            modelBuilder.Entity<BookAuthor>().HasOne(x => x.Author).WithMany(x => x.BookAuthors).HasForeignKey(x => x.AuthorId);
            modelBuilder.Entity<BookAuthor>().HasOne(x => x.Book).WithMany(x => x.BookAuthors).HasForeignKey(x => x.BookId);

            // User Modules many to many setup
            modelBuilder.Entity<UserHasModuleAddOn>().HasKey(x => new { x.UserId, x.ModuleAddOnId });
            modelBuilder.Entity<UserHasModuleAddOn>().HasOne(x => x.User).WithMany(x => x.ModuleAddOns).HasForeignKey(x => x.UserId);
            modelBuilder.Entity<UserHasModuleAddOn>().HasOne(x => x.ModuleAddOn).WithMany(x => x.ModuleAddOns).HasForeignKey(x => x.ModuleAddOnId);

            // Data Seeding
            SeedGenres(modelBuilder);
        }

        #region -> Database tables

        public DbSet<Notes> Notes { get; set; }

        public DbSet<Weight> Weight { get; set; }

        #region -> Modules

        public DbSet<ModuleAddOn> Modules { get; set; }

        public DbSet<UserHasModuleAddOn> UserHasModule { get; set; }

        #endregion

        #region -> Library 

        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<Bookshelf> Bookshelves { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<Series> Series { get; set; }

        public DbSet<BookAuthor> BookHasAuthors { get; set; }

        #endregion

        #region -> Identity

        public DbSet<WSUser> Users { get; set; }
        public DbSet<WSRole> Roles { get; set; }
        public DbSet<WSUserInRoles> UserRoles { get; set; }
        public DbSet<WSUserLogin> UserLogin { get; set; }
        public DbSet<WSUserTokens> UserTokens { get; set; }

        #endregion

        #endregion

        #region -> Private Functions

        private void SeedGenres (ModelBuilder modelBuilder)
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

        #endregion
    }
}
