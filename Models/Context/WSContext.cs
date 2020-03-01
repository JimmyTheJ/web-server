using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using VueServer.Models.Models.Library;
using VueServer.Models.User;

namespace VueServer.Models.Context
{
    public class WSContext : DbContext, IWSContext
    {
        public WSContext() { }

        public WSContext(DbContextOptions<WSContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<Book>().HasOne<Genre>().WithMany(x => x.Books).HasForeignKey(x => x.GenreId).IsRequired(false);
            modelBuilder.Entity<Book>().HasOne<Bookshelf>().WithMany(x => x.Books).HasForeignKey(x => x.BookshelfId).IsRequired(false);
            modelBuilder.Entity<Book>().Property(x => x.PublicationDate).IsRequired(false);

            //modelBuilder.Entity<Genre>().HasMany<Book>().WithOne(x => x.Genre).HasForeignKey(x => x.GenreId).IsRequired(false);
            //modelBuilder.Entity<Bookshelf>().HasMany<Book>().WithOne(x => x.Bookshelf).HasForeignKey(x => x.BookshelfId).IsRequired(false);

            //modelBuilder.Entity<SeriesItem>().HasOne<Series>().WithMany(x => x.SeriesItems).HasForeignKey("SeriesId").IsRequired(true);
        }

        #region -> Database tables

        public DbSet<Notes> Notes { get; set; }

        public DbSet<Weight> Weight { get; set; }

        #region -> Library 

        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<Bookshelf> Bookshelves { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<Series> Series { get; set; }

        public DbSet<SeriesItem> SeriesItems { get; set; }

        public DbSet<BookHasAuthor> BookHasAuthors { get; set; }

        #endregion

        #region -> Identity

        public DbSet<WSUser> Users { get; set; }
        public DbSet<WSRole> Roles { get; set; }
        public DbSet<WSUserInRoles> UserRoles { get; set; }
        public DbSet<WSUserLogin> UserLogin { get; set; }
        public DbSet<WSUserTokens> UserTokens { get; set; }

        #endregion

        #endregion
    }
}
