﻿using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models.Modules;
using VueServer.Modules.Library.Models;

namespace VueServer.Modules.Library.Context
{
    public class LibraryContext : WSContext, ILibraryContext
    {
        public LibraryContext() : base() { }
        public LibraryContext(DbContextOptions options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Bookcase> Bookcases { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<BookAuthor> BookHasAuthors { get; set; }
        public DbSet<BookGenre> BookHasGenres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Data Seeding
            SeedGenres(modelBuilder);
            SeedModule(modelBuilder);
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

        private void SeedModule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = LibraryConstants.ModuleAddOn.Id, Name = LibraryConstants.ModuleAddOn.Name });
        }
    }
}
