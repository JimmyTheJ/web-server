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

        #region -> Public Functions

        #region -> Book

        public async Task<IResult<IList<Book>>> GetAllBooks()
        {
            var books = await _wsContext.Books
                .Include(x => x.Bookcase)
                .Include(x => x.Series)
                .Include(x => x.BookAuthors)
                    .ThenInclude(x => x.Author)
                .Include(x => x.BookGenres)
                    .ThenInclude(x => x.Genre)
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

            // Create new book object to clean out anything we don't want from the request
            var newBook = new Book(request.Book);
            newBook.UserId = _user.Name;

            // One to many relationship changes
            var bookcase = await UpdateBookcaseConnectionAsync(request.Bookcase, newBook);
            var series = await UpdateSeriesConnectionAsync(request.Series, newBook);
            var shelf = await UpdateShelfConnectionAsync(request.Shelf, newBook);

            // Make or remove foreign key connections
            newBook.BookcaseId = bookcase?.Id ?? null;
            newBook.SeriesId = series?.Id ?? null;
            newBook.ShelfId = shelf?.Id ?? null;

            
            // Add book to database and save
            _wsContext.Books.Add(newBook);
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("CreateBook: Successfully added book to database");
            }
            catch
            {
                _logger.LogError("CreateBook: Error saving database on adding book");
                return new Result<Book>(null, SERVER_ERROR);
            }

            // Many to many relationship changes that require the new books Id
            var authors = await UpdateBookAuthorListAsync(request.Authors, newBook);
            var genres = await UpdateBookGenreListAsync(request.Genres, newBook);

            // Populate the response with the new objects
            newBook.BookAuthors = authors;
            newBook.Bookcase = bookcase;
            newBook.BookGenres = genres;
            newBook.Shelf = shelf;
            newBook.Series = series;

            return new Result<Book>(newBook, OK);
        }

        public async Task<IResult<Book>> UpdateBook(BookAddRequest request)
        {
            // Null request or null book request bad request
            if (request == null) return new Result<Book>(null, BAD_REQUEST);
            if (request.Book == null) return new Result<Book>(null, BAD_REQUEST);

            var oldBook = _wsContext.Books
                .Include(x => x.BookAuthors)
                    .ThenInclude(x => x.Author)
                .Include(x => x.BookGenres)
                    .ThenInclude(x => x.Genre)
                .Where(x => x.Id == request.Book.Id)
                .FirstOrDefault();

            if (oldBook == null)
            {
                // Can't update a book that doesn't exist in the database
                return new Result<Book>(null, BAD_REQUEST);
            }
            
            // One to many relationship changes
            var bookcase = await UpdateBookcaseConnectionAsync(request.Bookcase, oldBook);
            var series = await UpdateSeriesConnectionAsync(request.Series, oldBook);
            var shelf = await UpdateShelfConnectionAsync(request.Shelf, oldBook);
            
            // Many to many relationship changes
            var authors = await UpdateBookAuthorListAsync(request.Authors, oldBook);
            var genres = await UpdateBookGenreListAsync(request.Genres, oldBook);

            // Update book details
            oldBook.Boxset = request.Book.Boxset;
            oldBook.Edition = request.Book.Edition;
            oldBook.Hardcover = request.Book.Hardcover;
            oldBook.IsRead = request.Book.IsRead;
            oldBook.Loaned = request.Book.Loaned;
            oldBook.Notes = request.Book.Notes;
            oldBook.PublicationDate = request.Book.PublicationDate;
            oldBook.SeriesNumber = request.Book.SeriesNumber;
            oldBook.SubTitle = request.Book.SubTitle;
            oldBook.Title = request.Book.Title;

            // Make or remove foreign key connections
            oldBook.BookcaseId = bookcase?.Id ?? null;
            oldBook.SeriesId = series?.Id ?? null;
            oldBook.ShelfId = shelf?.Id ?? null;

            // Save database
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateBook: Successfully edited book in database");
            }
            catch
            {
                _logger.LogError("UpdateBook: Error saving database on editing book");
                return new Result<Book>(null, SERVER_ERROR);
            }

            // Populate the response with the new objects
            oldBook.BookAuthors = authors;
            oldBook.Bookcase = bookcase;
            oldBook.BookGenres = genres;
            oldBook.Shelf = shelf;
            oldBook.Series = series;

            return new Result<Book>(oldBook, OK);
        }

        public async Task<IResult<int>> DeleteBook (int id)
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
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("DeleteBook: Successfully deleted book from database");
            }
            catch
            {
                _logger.LogError("DeleteBook: Error saving database on deleting book");
                return new Result<int>(id, SERVER_ERROR);
            }

            return new Result<int>(id, OK);
        }

        #endregion

        #region -> Author

        public async Task<IResult<IList<Author>>> GetAllAuthors()
        {
            var authors = await _wsContext.Authors.ToListAsync();

            return new Result<IList<Author>>(authors, Domain.Enums.StatusCode.OK);
        }

        public Task<IResult<Author>> CreateAuthor(Author request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<Author>> UpdateAuthor(Author request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<int>> DeleteAuthor(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region -> Bookcase

        public async Task<IResult<IList<Bookcase>>> GetAllBookcases()
        {
            var bookshelves = await _wsContext.Bookcases.ToListAsync();

            return new Result<IList<Bookcase>>(bookshelves, Domain.Enums.StatusCode.OK);
        }

        public Task<IResult<Bookcase>> CreateBookcase(Bookcase request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<Bookcase>> UpdateBookcase(Bookcase request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<int>> DeleteBookcase(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region -> Genre

        public async Task<IResult<IList<Genre>>> GetAllGenres()
        {
            var genres = await _wsContext.Genres.ToListAsync();

            return new Result<IList<Genre>>(genres, Domain.Enums.StatusCode.OK);
        }

        public Task<IResult<Genre>> CreateGenre(Genre request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<Genre>> UpdateGenre(Genre request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<int>> DeleteGenre(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region -> Series

        public async Task<IResult<IList<Series>>> GetAllSeries()
        {
            var series = await _wsContext.Series.ToListAsync();

            return new Result<IList<Series>>(series, Domain.Enums.StatusCode.OK);
        }

        public Task<IResult<Series>> CreateSeries(Series request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<Series>> UpdateSeries(Series request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<int>> DeleteSeries(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region -> Shelf

        public async Task<IResult<IList<Shelf>>> GetAllShelves()
        {
            var shelves = await _wsContext.Shelves.ToListAsync();

            return new Result<IList<Shelf>>(shelves, Domain.Enums.StatusCode.OK);
        }

        public Task<IResult<Shelf>> CreateShelf(Shelf request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<Shelf>> UpdateShelf(Shelf request)
        {
            throw new NotImplementedException();
        }

        public Task<IResult<int>> DeleteShelf(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region -> Private Functions

        #region -> Author

        private async Task<IList<Author>> AddAuthorListAsync(IList<Author> authors)
        {
            if (authors == null || authors.Count == 0)
            {
                _logger.LogDebug("AddAuthorListAsync: Author list is null");
                return null;
            }
            int addedAuthors = 0;

            // Add new authors to the database
            foreach (var author in authors)
            {
                if (author.Id != 0)
                {
                    continue;
                }

                addedAuthors++;
                _wsContext.Authors.Add(author);
            }

            if (addedAuthors > 0)
            {
                try
                {
                    await _wsContext.SaveChangesAsync();
                    _logger.LogDebug("AddAuthorListAsync: Successfully added authors to database");
                }
                catch
                {
                    _logger.LogError("AddAuthorListAsync: Error saving database on adding authors");
                    return null;
                }

                return authors;
            }

            return null;
        }

        private async Task<IList<BookAuthor>> AddBookAuthorListAsync(IList<Author> authors, Book book)
        {
            if (authors == null || authors.Count == 0)
            {
                _logger.LogDebug("AddBookAuthorListAsync: Author list is null");
                return null;
            }
            var authorsToAdd = new List<Author>();

            var newAuthors = await AddAuthorListAsync(authors);
            if (newAuthors != null)
            {
                foreach (var author in newAuthors)
                {
                    authorsToAdd.Add(author);
                }
            }

            // Existing authors
            foreach (var author in authors)
            {
                if (author.Id > 0)
                {
                    authorsToAdd.Add(author);
                }
            }

            var newBookAuthors = await UpdateBookAuthorListConnectionsAsync(authorsToAdd, null, null, book);
            return newBookAuthors;
        }

        private async Task<IList<BookAuthor>> UpdateBookAuthorListConnectionsAsync(IList<Author> newAuthors, IList<BookAuthor> existingBookAuthors, IList<BookAuthor> bookAuthorsToDelete, Book book)
        {
            var bookAuthors = new List<BookAuthor>();

            // Add / Update all the Book Author connections
            if (newAuthors != null)
            {
                foreach (var author in newAuthors)
                {
                    bookAuthors.Add(new BookAuthor() { AuthorId = author.Id, BookId = book.Id });
                }
            }

            foreach (var bA in bookAuthors)
            {
                _wsContext.BookHasAuthors.Add(bA);
            }

            if (existingBookAuthors != null)
            {
                foreach (var bA in existingBookAuthors)
                {
                    bookAuthors.Add(bA);
                }
            }            

            if (bookAuthorsToDelete != null)
            {
                foreach (var author in bookAuthorsToDelete)
                {
                    _wsContext.BookHasAuthors.Remove(author);
                }
            }

            // TODO: Check if all the parameters are null maybe ?
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateBookAuthorListConnectionsAsync: Successfully made book author connections in the database");
            }
            catch
            {
                _logger.LogError("UpdateBookAuthorListConnectionsAsync: Error saving database on book author connections");
                return null;
            }

            return bookAuthors;
        }

        private async Task<IList<BookAuthor>> UpdateBookAuthorListAsync(IList<Author> authors, Book book)
        {
            if (authors == null || authors.Count == 0)
            {
                _logger.LogDebug("UpdateBookAuthorListAsync: Author list is null");
                return null;
            }

            if (book == null)
            {
                _logger.LogDebug("UpdateBookAuthorListAsync: Book is null");
                return null;
            }

            var existingBookAuthors = await _wsContext.BookHasAuthors.Include(x => x.Author).Where(x => x.BookId == book.Id && authors.Select(y => y.Id).Any(y => y == x.AuthorId)).ToListAsync();
            var addAuthors = authors.Where(x => !existingBookAuthors.Any(y => y.AuthorId == x.Id)).ToList();
            var bookAuthorsToDelete = book.BookAuthors?.Where(x => !existingBookAuthors.Any(y => y.AuthorId == x.AuthorId) && !addAuthors.Any(y => y.Id == x.AuthorId)).ToList();

            var newAuthors = await AddAuthorListAsync(addAuthors);
            var newBookAuthors = await UpdateBookAuthorListConnectionsAsync(newAuthors ?? addAuthors, existingBookAuthors, bookAuthorsToDelete, book);

            return newBookAuthors;
        }

        #endregion

        #region -> Bookcase

        private async Task<Bookcase> UpdateBookcaseConnectionAsync(Bookcase bookcase, Book book)
        {
            if (bookcase == null)
            {
                _logger.LogDebug("UpdateBookcaseConnectionAsync: Bookcase is null");
                return null;
            }

            if (book == null)
            {
                _logger.LogDebug("UpdateBookcaseConnectionAsync: Book is null");
                return null;
            }

            if (bookcase.Id <= 0)
            {
                _logger.LogDebug("UpdateBookcaseConnectionAsync: Bookcase Id is less than or equal to 0");
                var newBookcase = await AddBookcaseAsync(bookcase);
                return newBookcase;
            }

            var existingBookcase = await _wsContext.Bookcases.Where(x => x.Id == bookcase.Id).FirstOrDefaultAsync();
            if (existingBookcase == null)
            {
                _logger.LogDebug("UpdateBookcaseConnectionAsync: No existing bookcase exists");
                var newBookcase = await AddBookcaseAsync(bookcase);
                return newBookcase;
            }

            // Bookcase hasn't changed
            if (existingBookcase.Id == book.BookcaseId)
            {
                _logger.LogDebug("UpdateBookcaseConnectionAsync: Bookcase's connection hasn't changed");
                return existingBookcase;
            }

            _logger.LogDebug("UpdateBookcaseConnectionAsync: Bookcase's connection changed");
            return existingBookcase;
        }

        private async Task<Bookcase> AddBookcaseAsync (Bookcase bookcase)
        {
            if (bookcase == null)
            {
                _logger.LogDebug("AddBookcaseAsync: Bookcase is null");
                return null;
            }

            if (bookcase.Id != 0)
            {
                bookcase.Id = 0;
            }
                
            _wsContext.Bookcases.Add(bookcase);
            try
            {
                var result = await _wsContext.SaveChangesAsync();
                _logger.LogDebug("AddBookcaseAsync: Successfully added bookcase to database");
                return bookcase;
            }
            catch
            {
                _logger.LogError("AddBookcaseAsync: Error saving database");
                return null;
            }
        }

        private async Task<Bookcase> EditBookcaseAsync(Bookcase bookcase)
        {
            if (bookcase == null)
            {
                _logger.LogDebug("EditBookcaseAsync: Bookcase is null");
                return null;
            }

            if (bookcase.Id != 0)
            {
                bookcase.Id = 0;
            }

            var oldBookcase = await _wsContext.Bookcases.Where(x => x.Id == bookcase.Id).FirstOrDefaultAsync();
            if (oldBookcase == null)
            {
                _logger.LogDebug("EditBookcaseAsync: Bookcase doesn't exist in database");
                return null;
            }

            oldBookcase.Name = bookcase.Name;
            try
            {
                var result = await _wsContext.SaveChangesAsync();
                _logger.LogDebug("EditBookcaseAsync: Successfully updated bookcase in database");
                return bookcase;
            }
            catch
            {
                _logger.LogError("EditBookcaseAsync: Error saving database");
                return null;
            }
        }

        #endregion

        #region -> Genre

        private async Task<IList<BookGenre>> AddBookGenreListAsync(IList<Genre> genres, Book book)
        {
            if (genres == null || genres.Count == 0)
            {
                _logger.LogDebug("AddBookGenreListAsync: Genre list is null");
                return null;
            }

            var newBookGenres = await UpdateBookGenreListConnectionsAsync(genres, null, null, book);
            return newBookGenres;
        }

        private async Task<IList<BookGenre>> UpdateBookGenreListConnectionsAsync(IList<Genre> newGenres, IList<BookGenre> existingBookGenres, IList<BookGenre> bookGenresToDelete, Book book)
        {
            var bookGenres = new List<BookGenre>();

            // Add / Update all the Book Genre connections
            if (newGenres != null)
            {
                foreach (var genre in newGenres)
                {
                    bookGenres.Add(new BookGenre() { GenreId = genre.Id, BookId = book.Id });
                }
            }

            foreach (var bG in bookGenres)
            {
                _wsContext.BookHasGenres.Add(bG);
            }

            if (existingBookGenres != null)
            {
                foreach (var bG in existingBookGenres)
                {
                    bookGenres.Add(bG);
                }
            }

            if (bookGenresToDelete != null)
            {
                foreach (var genre in bookGenresToDelete)
                {
                    _wsContext.BookHasGenres.Remove(genre);
                }
            }

            // TODO: Check if all the parameters are null maybe ?
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateBookGenreListConnectionsAsync: Successfully made book genre connections in the database");
            }
            catch
            {
                _logger.LogError("UpdateBookGenreListConnectionsAsync: Error saving database on book genre connections");
                return null;
            }

            return bookGenres;
        }

        private async Task<IList<BookGenre>> UpdateBookGenreListAsync(IList<Genre> genres, Book book)
        {
            if (genres == null || genres.Count == 0)
            {
                _logger.LogDebug("UpdateBookGenreListAsync: Genre list is null");
                return null;
            }

            if (book == null)
            {
                _logger.LogDebug("UpdateBookGenreListAsync: Book is null");
                return null;
            }

            var existingBookGenres = await _wsContext.BookHasGenres.Include(x => x.Genre).Where(x => x.BookId == book.Id && genres.Select(y => y.Id).Any(y => y == x.GenreId)).ToListAsync();
            var addGenres = genres.Where(x => !existingBookGenres.Any(y => y.GenreId == x.Id)).ToList();
            var bookGenresToDelete = book.BookGenres?.Where(x => !existingBookGenres.Any(y => y.GenreId == x.GenreId) && !addGenres.Any(y => y.Id == x.GenreId)).ToList();

            var newBookGenres = await UpdateBookGenreListConnectionsAsync(addGenres, existingBookGenres, bookGenresToDelete, book);

            return newBookGenres;
        }

        #endregion

        #region -> Shelf

        private async Task<Shelf> UpdateShelfConnectionAsync(Shelf shelf, Book book)
        {
            if (shelf == null)
            {
                _logger.LogDebug("UpdateShelfConnectionAsync: Shelf is null");
                return null;
            }

            if (book == null)
            {
                _logger.LogDebug("UpdateShelfConnectionAsync: Book is null");
                return null;
            }

            if (shelf.Id <= 0)
            {
                _logger.LogDebug("UpdateShelfConnectionAsync: Shelf Id is less than or equal to 0");
                var newShelf = await AddShelfAsync(shelf);
                return newShelf;
            }

            var existingShelf = await _wsContext.Shelves.Where(x => x.Id == shelf.Id).FirstOrDefaultAsync();
            if (existingShelf == null)
            {
                _logger.LogDebug("UpdateShelfConnectionAsync: No existing shelf exists");
                var newShelf = await AddShelfAsync(shelf);
                return newShelf;
            }

            // Shelf hasn't changed
            if (existingShelf.Id == book.ShelfId)
            {
                _logger.LogDebug("UpdateShelfConnectionAsync: Shelf's connection hasn't changed");
                return existingShelf;
            }

            _logger.LogDebug("UpdateShelfConnectionAsync: Shelf's connection changed");
            return existingShelf;
        }

        private async Task<Shelf> AddShelfAsync(Shelf shelf)
        {
            if (shelf == null)
            {
                _logger.LogDebug("AddShelfAsync: Shelf is null");
                return null;
            }

            if (shelf.Id != 0)
            {
                shelf.Id = 0;
            }

            _wsContext.Shelves.Add(shelf);
            try
            {
                var result = await _wsContext.SaveChangesAsync();
                _logger.LogDebug("AddShelfAsync: Successfully added shelf to database");
                return shelf;
            }
            catch
            {
                _logger.LogError("AddShelfAsync: Error saving database");
                return null;
            }
        }

        private async Task<Shelf> EditShelfAsync(Shelf shelf)
        {
            if (shelf == null)
            {
                _logger.LogDebug("EditShelfAsync: Shelf is null");
                return null;
            }

            var oldShelf = await _wsContext.Shelves.Where(x => x.Id == shelf.Id).FirstOrDefaultAsync();
            if (oldShelf == null)
            {
                _logger.LogDebug("EditShelfAsync: Shelf doesn't exist in database");
                return null;
            }

            // Shelf has moved bookcases
            if (shelf.BookcaseId != oldShelf.BookcaseId)
            {
                _logger.LogDebug("EditShelfAsync: Attempting to change shelf to a new bookcase");
                var newBookcase = await _wsContext.Bookcases.Where(x => x.Id == shelf.BookcaseId).FirstOrDefaultAsync();

                // New bookcase doesn't exit
                if (newBookcase == null)
                {
                    _logger.LogDebug("EditShelfAsync: Can't change shelf to a non-existant bookcase");
                    return oldShelf;
                }

                oldShelf.BookcaseId = newBookcase.Id;
            }

            oldShelf.Name = shelf.Name;
            try
            {
                var result = await _wsContext.SaveChangesAsync();
                _logger.LogDebug("EditShelfAsync: Successfully updated shelf in database");
                return shelf;
            }
            catch
            {
                _logger.LogError("EditShelfAsync: Error saving database");
                return null;
            }
        }

        #endregion

        #region -> Series

        private async Task<Series> UpdateSeriesConnectionAsync(Series series, Book book)
        {
            if (series == null)
            {
                _logger.LogDebug("UpdateSeriesConnectionAsync: Series is null");
                return null;
            }

            if (book == null)
            {
                _logger.LogDebug("UpdateSeriesConnectionAsync: Book is null");
                return null;
            }

            if (series.Id <= 0)
            {
                _logger.LogDebug("UpdateSeriesConnectionAsync: Series Id is less than or equal to 0");
                var newSeries = await AddSeriesAsync(series);
                return newSeries;
            }

            var existingSeries = await _wsContext.Series.Where(x => x.Id == series.Id).FirstOrDefaultAsync();
            if (existingSeries == null)
            {
                _logger.LogDebug("UpdateSeriesConnectionAsync: No existing series exists");
                var newSeries = await AddSeriesAsync(series);
                return newSeries;
            }

            // Series hasn't changed
            if (existingSeries.Id == book.SeriesId)
            {
                _logger.LogDebug("UpdateSeriesConnectionAsync: Series's connection hasn't changed");
                return existingSeries;
            }

            _logger.LogDebug("UpdateSeriesConnectionAsync: Series's connection changed");
            return existingSeries;
        }

        private async Task<Series> AddSeriesAsync (Series series)
        {
            if (series == null)
            {
                _logger.LogDebug("AddSeriesAsync: Series is null");
                return null;
            }

            if (series.Id != 0)
            {
                series.Id = 0;
            }
                
            _wsContext.Series.Add(series);
            try
            {
                var result = await _wsContext.SaveChangesAsync();
                _logger.LogDebug("AddSeriesAsync: Successfully added series to database");
                return series;
            }
            catch
            {
                _logger.LogError("AddSeriesAsync: Error saving database");
                return null;
            }
        }

        private async Task<Series> EditSeriesAsync(Series series)
        {
            if (series == null)
            {
                _logger.LogDebug("EditSeriesAsync: Series is null");
                return null;
            }

            var oldSeries = await _wsContext.Series.Where(x => x.Id == series.Id).FirstOrDefaultAsync();
            if (oldSeries == null)
            {
                _logger.LogDebug("EditSeriesAsync: Series doesn't exist in database");
                return null;
            }

            oldSeries.Active = series.Active;
            oldSeries.Name = series.Name;
            oldSeries.Number = series.Number;
            try
            {
                var result = await _wsContext.SaveChangesAsync();
                _logger.LogDebug("EditSeriesAsync: Successfully updated series in database");
                return series;
            }
            catch
            {
                _logger.LogError("EditSeriesAsync: Error saving database");
                return null;
            }
        }

        #endregion

        #endregion
    }
}
