using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Domain.Concrete;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Context;
using VueServer.Models.Library;
using VueServer.Models.Request;
using VueServer.Services.Interface;
using static VueServer.Domain.Enums.StatusCode;

namespace VueServer.Services.Concrete
{
    public class LibraryService : ILibraryService
    {
        private readonly ILogger _logger;

        private readonly IUserService _user;

        private readonly IWebHostEnvironment _env;

        private readonly IWSContext _wsContext;

        public LibraryService(ILoggerFactory logger, IUserService user, IWebHostEnvironment env, IWSContext wsContext)
        {
            _logger = logger?.CreateLogger<LibraryService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _wsContext = wsContext ?? throw new ArgumentNullException("WSContext null");
        }

        public async Task<IResult<IList<Book>>> GetAllBooks()
        {
            var books = await _wsContext.Books
                .Include(x => x.Bookshelf)
                .Include(x => x.Genre)
                .Include(x => x.Series)
                .Include(x => x.BookAuthors)
                .ThenInclude(x => x.Author)
                .ToListAsync();

            return new Result<IList<Book>>(books, OK);
        }

        public async Task<IResult<Book>> GetBook(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IResult<Book>> CreateBook(BookAddRequest request)
        {
            // Null request or null book request bad request
            if (request == null) return new Result<Book>(null, BAD_REQUEST);
            if (request.Book == null) return new Result<Book>(null, BAD_REQUEST);

            IList<Author> existingAuthors = new List<Author>();
            IList<Author> newAuthors = new List<Author>();

            if (request.Bookshelf != null)
            {
                if (_wsContext.Bookshelves.Where(x => x.Id == request.Bookshelf.Id).FirstOrDefault() == null)
                {
                    _wsContext.Bookshelves.Add(request.Bookshelf);
                }
            }

            if (request.Series != null)
            {
                if (_wsContext.Series.Where(x => x.Id == request.Series.Id).FirstOrDefault() == null)
                {
                    _wsContext.Series.Add(request.Series);
                }
            }
            
            if (request.Authors != null && request.Authors.Count > 0)
            {
                existingAuthors = await _wsContext.Authors.Where(x => request.Authors.Select(y => y.Id).Any(y => x.Id == y)).ToListAsync();
                newAuthors = request.Authors.Where(x => !existingAuthors.Any(y => y.Id == x.Id)).ToList();

                foreach (var author in newAuthors)
                {
                    _wsContext.Authors.Add(author);
                }
            }
            await _wsContext.SaveChangesAsync();

            // Add the book to the database
            var newBook = new Book(request.Book);
            newBook.UserId = _user.Name;
            newBook.BookshelfId = request.Bookshelf?.Id ?? request.BookShelfId;
            newBook.SeriesId = request.Series?.Id ?? request.SeriesId;
            newBook.GenreId = request.GenreId;

            _wsContext.Books.Add(newBook);
            // Save database so we can use the book id for the adding of authors
            await _wsContext.SaveChangesAsync();

            // Add the new authors many to many relationship for this book
            if (newAuthors != null)
            {
                foreach (var author in newAuthors)
                {
                    _wsContext.BookHasAuthors.Add(new BookAuthor() { AuthorId = author.Id, BookId = newBook.Id });
                }
            }

            // Add the existing authors many to many relationship for this book
            if (existingAuthors != null)
            {
                foreach (var author in existingAuthors)
                {
                    _wsContext.BookHasAuthors.Add(new BookAuthor() { AuthorId = author.Id, BookId = newBook.Id });
                }
            }

            await _wsContext.SaveChangesAsync();

            newBook.Bookshelf = request.Bookshelf;
            newBook.Series = request.Series;

            return new Result<Book>(newBook, OK);
        }

        public async Task<IResult<Book>> UpdateBook(BookAddRequest request)
        {
            // Null request or null book request bad request
            if (request == null) return new Result<Book>(null, BAD_REQUEST);
            if (request.Book == null) return new Result<Book>(null, BAD_REQUEST);

            IList<Author> newAuthors = new List<Author>();
            IList<BookAuthor> authorsToDelete = new List<BookAuthor>();
            Bookshelf newBookshelf = null;
            Series newSeries = null;

            var oldBook = _wsContext.Books.Include(x => x.BookAuthors).Where(x => x.Id == request.Book.Id).FirstOrDefault();
            if (oldBook == null)
            {
                // Can't update a book that doesn't exist in the database
                return new Result<Book>(null, BAD_REQUEST);
            }


            // Bookshelf code
            if (request.Bookshelf != null)
            {
                // New bookshelf
                if (_wsContext.Bookshelves.Where(x => x.Id == request.Bookshelf.Id).FirstOrDefault() == null)
                {
                    newBookshelf = new Bookshelf(request.Bookshelf);
                    _wsContext.Bookshelves.Add(newBookshelf);
                }
                else
                {
                    // Presumably impossible
                    oldBook.BookshelfId = request.Bookshelf.Id;
                }
            }
            else if (request.BookShelfId > 0)
            {
                // Add existing bookshelf (or change from a different bookshelf)
                if (_wsContext.Bookshelves.Where(x => x.Id == request.BookShelfId).FirstOrDefault() != null) 
                {
                    oldBook.BookshelfId = request.BookShelfId;
                }
                else
                {
                    // Tried to add the book to a non-existant bookshelf
                }
            }
            else if (oldBook.BookshelfId > 0 && (request.BookShelfId == null || request.BookShelfId == 0) )
            {
                // Delete bookshelf
                oldBook.BookshelfId = null;
            }


            // Series code
            if (request.Series != null)
            {
                // New series
                if (_wsContext.Series.Where(x => x.Id == request.Series.Id).FirstOrDefault() == null)
                {
                    newSeries = new Series(request.Series);
                    _wsContext.Series.Add(newSeries);
                }
                else
                {
                    // Presumably impossible
                    oldBook.SeriesId = request.Series.Id;
                }
            }
            else if (request.SeriesId > 0)
            {
                // Add existing series (or change from a different series)
                if (_wsContext.Series.Where(x => x.Id == request.SeriesId).FirstOrDefault() != null)
                {
                    oldBook.SeriesId = request.SeriesId;
                }
                else
                {
                    // Tried to add the book to a non-existant series
                }
            }
            else if (oldBook.SeriesId > 0 && (request.SeriesId == null || request.SeriesId == 0))
            {
                // Delete Series
                oldBook.SeriesId = null;
                request.Book.SeriesNumber = 0;
            }

            // Author code
            if (request.Authors != null && request.Authors.Count > 0)
            {
                var existingBookAuthors = await _wsContext.BookHasAuthors.Include(x => x.Author).Where(x => x.BookId == oldBook.Id && request.Authors.Select(y => y.Id).Any(y => y == x.AuthorId)).ToListAsync();
                newAuthors = request.Authors.Where(x => !existingBookAuthors.Any(y => y.AuthorId == x.Id)).ToList();
                authorsToDelete = oldBook.BookAuthors.Where(x => !existingBookAuthors.Any(y => y.AuthorId == x.AuthorId) && !newAuthors.Any(y => y.Id == x.AuthorId)).ToList();

                // TODO: Need logic to remove authors that are already there
                

                foreach (var author in newAuthors)
                {
                    // If these authors need to be created do so here
                    if (author.Id <= 0)
                    {
                        _wsContext.Authors.Add(author);
                    }
                    else
                    {
                        // We aren't creating anything if they aren't exist. Just need to make the BookAuthor connections later
                    }
                }
            }

            oldBook.Edition = request.Book.Edition;
            oldBook.Hardcover = request.Book.Hardcover;
            oldBook.IsRead = request.Book.IsRead;
            oldBook.PublicationDate = request.Book.PublicationDate;
            oldBook.SubTitle = request.Book.SubTitle;
            oldBook.Title = request.Book.Title;
            oldBook.GenreId = request.GenreId;
            oldBook.SeriesNumber = request.Book.SeriesNumber;

            // Save database so we can update the bookshelf id and series id
            await _wsContext.SaveChangesAsync();

            if (newBookshelf != null)
            {
                oldBook.BookshelfId = newBookshelf.Id;
            }

            if (newSeries != null)
            {
                oldBook.SeriesId = newSeries.Id;
            }

            if (newAuthors != null)
            {
                foreach (var author in newAuthors)
                {
                    // TODO: This might be adding extra authors
                    _wsContext.BookHasAuthors.Add(new BookAuthor()  { AuthorId = author.Id, BookId = oldBook.Id });
                }
            }

            if (authorsToDelete != null)
            {
                foreach (var author in authorsToDelete)
                {
                    _wsContext.BookHasAuthors.Remove(author);
                }
            }

            await _wsContext.SaveChangesAsync();

            // Populate the response with the new authors
            foreach (var bookAuthor in oldBook.BookAuthors)
            {
                if (bookAuthor.Author == null)
                {
                    bookAuthor.Author = request.Authors.Where(x => x.Id == bookAuthor.AuthorId).FirstOrDefault();
                }
            }

            if (oldBook.BookshelfId != null)
            {
                oldBook.Bookshelf = _wsContext.Bookshelves.Where(x => x.Id == oldBook.BookshelfId).FirstOrDefault();
            }
            
            if (oldBook.SeriesId != null)
            {
                oldBook.Series = _wsContext.Series.Where(x => x.Id == oldBook.SeriesId).FirstOrDefault();
            }

            return new Result<Book>(oldBook, OK);
        }

        public async Task<IResult> DeleteBook (int id)
        {
            var book = await _wsContext.Books.Include(x => x.BookAuthors).Where(x => x.Id == id).FirstOrDefaultAsync();

            if (book.BookAuthors != null)
            {
                foreach (var bookAuthor in book.BookAuthors)
                {
                    _wsContext.BookHasAuthors.Remove(bookAuthor);
                }
            }            

            _wsContext.Books.Remove(book);
            await _wsContext.SaveChangesAsync();

            return new Result<IResult>(null, OK);
        }

        public async Task<IResult<IList<Genre>>> GetAllGenres()
        {
            var genres = await _wsContext.Genres.ToListAsync();

            return new Result<IList<Genre>>(genres, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IList<Author>>> GetAllAuthors()
        {
            var authors = await _wsContext.Authors.ToListAsync();

            return new Result<IList<Author>>(authors, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IList<Series>>> GetAllSeries()
        {
            var series = await _wsContext.Series.ToListAsync();

            return new Result<IList<Series>>(series, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IList<Bookshelf>>> GetAllBookshelves()
        {
            var bookshelves = await _wsContext.Bookshelves.ToListAsync();

            return new Result<IList<Bookshelf>>(bookshelves, Domain.Enums.StatusCode.OK);
        }
    }
}
