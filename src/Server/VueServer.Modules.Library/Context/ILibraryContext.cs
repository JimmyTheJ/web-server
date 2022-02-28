using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Library.Models;

namespace VueServer.Modules.Library.Context
{
    public interface ILibraryContext : IWSContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Bookcase> Bookcases { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<BookAuthor> BookHasAuthors { get; set; }
        public DbSet<BookGenre> BookHasGenres { get; set; }
    }
}
