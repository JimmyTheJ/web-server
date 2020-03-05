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
            var books = await _wsContext.Books.ToListAsync();

            return new Result<IList<Book>>(books, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Book>> GetBook(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IResult<Book>> CreateBook(BookAddRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<IResult<Book>> UpdateBook(BookAddRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<IResult> DeleteBook (int id)
        {
            throw new NotImplementedException();
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
