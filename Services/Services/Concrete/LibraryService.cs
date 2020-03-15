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
using VueServer.Models.Models.Library;
using VueServer.Models.Models.Request;
using static VueServer.Domain.Enums.StatusCode;

namespace VueServer.Services.Interface
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
                .Include(x => x.Authors)
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

            IList<Author> newAuthors = new List<Author>();
            Bookshelf newBookshelf = null;
            Series newSeries = null;

            if (request.Bookshelf != null)
            {
                if (_wsContext.Bookshelves.Where(x => x.Id == request.Bookshelf.Id).FirstOrDefault() == null)
                {
                    newBookshelf = new Bookshelf(request.Bookshelf);
                    _wsContext.Bookshelves.Add(newBookshelf);
                }
                else
                {
                    request.Book.BookshelfId = request.Bookshelf.Id;
                }
            }

            if (request.Series != null)
            {
                if (_wsContext.Series.Where(x => x.Id == request.Series.Id).FirstOrDefault() == null)
                {
                    newSeries = new Series(request.Series);
                    _wsContext.Series.Add(newSeries);
                }
                else
                {
                    request.Book.SeriesId = request.Series.Id;
                }
            }
            
            if (request.Authors != null)
            {
                foreach (var author in request.Authors)
                {
                    if (_wsContext.Authors.Where(x => x.Id == author.Id).FirstOrDefault() == null)
                    {
                        var a = new Author(author);
                        newAuthors.Add(a);
                        _wsContext.Authors.Add(a);
                    }
                }
            }
            // Save database so we have correct ids for all the above objects
            await _wsContext.SaveChangesAsync();

            // Add the book to the database
            var newBook = new Book(request.Book);
            newBook.UserId = _user.Name;
            newBook.BookshelfId = newBookshelf?.Id ?? newBook.BookshelfId;
            newBook.SeriesId = newSeries?.Id ?? newBook.SeriesId;

            if (request.GenreId > 0)
                newBook.GenreId = request.GenreId;

            _wsContext.Books.Add(newBook);
            // Save database so we can use the book id for the adding of authors
            await _wsContext.SaveChangesAsync();

            if (newAuthors != null && newAuthors.Count > 0)
            {
                foreach (var author in newAuthors)
                {
                    if (_wsContext.BookHasAuthors.Where(x => x.BookId == newBook.Id && x.AuthorId == author.Id).FirstOrDefault() == null)
                    {
                        _wsContext.BookHasAuthors.Add(new BookHasAuthor() { AuthorId = author.Id, BookId = newBook.Id });
                    }
                }
            }

            await _wsContext.SaveChangesAsync();

            newBook.Authors = newAuthors;
            return new Result<Book>(newBook, OK);
        }

        public async Task<IResult<Book>> UpdateBook(BookAddRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<IResult> DeleteBook (int id)
        {
            var book = _wsContext.Books.Where(x => x.Id == id).FirstOrDefault();

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
