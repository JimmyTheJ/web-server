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
                .Include(x => x.Shelf)
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
            newBook.UserId = _user.Id;

            // One to many relationship changes
            var bookcase = await UpdateBookcaseConnectionAsync(request.Bookcase, newBook);
            var series = await UpdateSeriesConnectionAsync(request.Series, newBook);
            var shelf = await ShelfInBookCaseCheck(request, newBook);

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
            var shelf = await ShelfInBookCaseCheck(request, oldBook);

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
            if (book == null)
            {
                _logger.LogDebug($"DeleteBook: Book not found with id ({id}). Bad request.");
                return new Result<int>(-1, BAD_REQUEST);
            }

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
                _logger.LogError($"DeleteAuthor: Error saving database on deleting author with id ({id})");
                return new Result<int>(id, SERVER_ERROR);
            }

            return new Result<int>(id, OK);
        }

        #endregion

        #region -> Author

        public async Task<IResult<IList<Author>>> GetAllAuthors()
        {
            var authors = await _wsContext.Authors.ToListAsync();

            return new Result<IList<Author>>(authors, OK);
        }

        public async Task<IResult<Author>> CreateAuthor(Author request)
        {
            if (request == null)
            {
                _logger.LogDebug("CreateAuthor: Author is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("CreateAuthor: Author failed model validation");
                return null;
            }

            _wsContext.Authors.Add(request);
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("CreateAuthor: Successfully added author to database");
            }
            catch
            {
                _logger.LogError("CreateAuthor: Error saving database on adding author");
                return null;
            }

            return new Result<Author>(request, OK);
        }

        public async Task<IResult<Author>> UpdateAuthor(Author request)
        {
            if (request == null)
            {
                _logger.LogDebug("UpdateAuthor: Author is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("UpdateAuthor: Author failed model validation");
                return null;
            }

            var oldAuthor = await _wsContext.Authors.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
            if (oldAuthor == null)
            {
                _logger.LogDebug($"UpdateAuthor: No author exists with id ({request.Id})");
                return null;
            }

            oldAuthor.FirstName = request.FirstName;
            oldAuthor.LastName = request.LastName;
            oldAuthor.Deceased = request.Deceased;
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateAuthor: Successfully updated author in database");
            }
            catch
            {
                _logger.LogError($"UpdateAuthor: Error saving database on updated author with id ({request.Id})");
                return null;
            }

            return new Result<Author>(oldAuthor, OK);
        }

        public async Task<IResult<int>> DeleteAuthor(int id)
        {
            var author = await _wsContext.Authors.Include(x => x.BookAuthors).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (author == null)
            {
                _logger.LogDebug($"DeleteAuthor: No author exists with id ({id})");
                return new Result<int>(-1, BAD_REQUEST);
            }

            // Delete author
            if (author.BookAuthors == null || author.BookAuthors.Count == 0)
            {
                _wsContext.Remove(author);
                try
                {
                    await _wsContext.SaveChangesAsync();
                }
                catch
                {
                    _logger.LogError($"DeleteAuthor: Error saving database on deleting author with id ({id})");
                    return new Result<int>(-1, SERVER_ERROR);
                }

                _logger.LogDebug("DeleteAuthor: Successfully deleted author from database");
                return new Result<int>(id, OK);
            }

            // Can't delete author because there is other connections at stake
            _logger.LogDebug($"DeleteAuthor: Can't delete the author with id ({id}) as it has other connections");
            return new Result<int>(-1, NO_CONTENT);
        }

        #endregion

        #region -> Bookcase

        public async Task<IResult<IList<Bookcase>>> GetAllBookcases()
        {
            var bookshelves = await _wsContext.Bookcases.ToListAsync();

            return new Result<IList<Bookcase>>(bookshelves, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Bookcase>> CreateBookcase(Bookcase request)
        {
            if (request == null)
            {
                _logger.LogDebug("CreateBookcase: Bookcase is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("CreateBookcase: Bookcase failed model validation");
                return null;
            }

            _wsContext.Bookcases.Add(request);
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("CreateBookcase: Successfully added bookcase to database");
            }
            catch
            {
                _logger.LogError("CreateBookcase: Error saving database on adding bookcase");
                return null;
            }

            return new Result<Bookcase>(request, OK);
        }

        public async Task<IResult<Bookcase>> UpdateBookcase(Bookcase request)
        {
            if (request == null)
            {
                _logger.LogDebug("UpdateBookcase: Bookcase is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("UpdateBookcase: Bookcase failed model validation");
                return null;
            }

            var oldBookcase = await _wsContext.Bookcases.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
            if (oldBookcase == null)
            {
                _logger.LogDebug($"UpdateBookcase: No bookcase exists with id ({request.Id})");
                return null;
            }

            oldBookcase.Name = request.Name;
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateBookcase: Successfully updated bookcase in database");
            }
            catch
            {
                _logger.LogError($"UpdateBookcase: Error saving database on updated bookcase with id ({request.Id})");
                return null;
            }

            return new Result<Bookcase>(oldBookcase, OK);
        }

        public async Task<IResult<int>> DeleteBookcase(int id)
        {
            var bookcase = await _wsContext.Bookcases.Include(x => x.Books).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (bookcase == null)
            {
                _logger.LogDebug($"DeleteBookcase: No bookcase exists with id ({id})");
                return new Result<int>(-1, BAD_REQUEST);
            }

            // Delete bookcase
            if ((bookcase.Books == null || bookcase.Books.Count == 0) && (bookcase.Shelves == null || bookcase.Shelves.Count == 0))
            {
                _wsContext.Remove(bookcase);
                try
                {
                    await _wsContext.SaveChangesAsync();
                }
                catch
                {
                    _logger.LogError($"DeleteBookcase: Error saving database on deleting bookcase with id ({id})");
                    return new Result<int>(-1, SERVER_ERROR);
                }

                _logger.LogDebug("DeleteBookcase: Successfully deleted bookcase from database");
                return new Result<int>(id, OK);
            }

            // Can't delete bookcase because there is other connections at stake
            _logger.LogDebug($"DeleteBookcase: Can't delete the bookcase with id ({id}) as it has other connections");
            return new Result<int>(-1, NO_CONTENT);
        }

        #endregion

        #region -> Genre

        public async Task<IResult<IList<Genre>>> GetAllGenres()
        {
            var genres = await _wsContext.Genres.ToListAsync();

            return new Result<IList<Genre>>(genres, Domain.Enums.StatusCode.OK);
        }

        #endregion

        #region -> Series

        public async Task<IResult<IList<Series>>> GetAllSeries()
        {
            var series = await _wsContext.Series.ToListAsync();

            return new Result<IList<Series>>(series, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Series>> CreateSeries(Series request)
        {
            if (request == null)
            {
                _logger.LogDebug("CreateSeries: Series is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("CreateSeries: Series failed model validation");
                return null;
            }

            _wsContext.Series.Add(request);
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("CreateSeries: Successfully added series to database");
            }
            catch
            {
                _logger.LogError("CreateSeries: Error saving database on adding series");
                return null;
            }

            return new Result<Series>(request, OK);
        }

        public async Task<IResult<Series>> UpdateSeries(Series request)
        {
            if (request == null)
            {
                _logger.LogDebug("UpdateSeries: Series is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("UpdateSeries: Series failed model validation");
                return null;
            }

            var oldSeries = await _wsContext.Series.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
            if (oldSeries == null)
            {
                _logger.LogDebug($"UpdateSeries: No series exists with id ({request.Id})");
                return null;
            }

            oldSeries.Active = request.Active;
            oldSeries.Name = request.Name;
            oldSeries.Number = request.Number;
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateSeries: Successfully updated series in database");
            }
            catch
            {
                _logger.LogError($"UpdateSeries: Error saving database on updated series with id ({request.Id})");
                return null;
            }

            return new Result<Series>(oldSeries, OK);
        }

        public async Task<IResult<int>> DeleteSeries(int id)
        {
            var series = await _wsContext.Series.Include(x => x.Books).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (series == null)
            {
                _logger.LogDebug($"DeleteSeries: No series exists with id ({id})");
                return new Result<int>(-1, BAD_REQUEST);
            }

            // Delete series
            if (series.Books == null || series.Books.Count == 0)
            {
                _wsContext.Remove(series);
                try
                {
                    await _wsContext.SaveChangesAsync();
                }
                catch
                {
                    _logger.LogError($"DeleteSeries: Error saving database on deleting series with id ({id})");
                    return new Result<int>(-1, SERVER_ERROR);
                }

                _logger.LogDebug("DeleteSeries: Successfully deleted series from database");
                return new Result<int>(id, OK);
            }

            // Can't delete series because there is other connections at stake
            _logger.LogDebug($"DeleteSeries: Can't delete the series with id ({id}) as it has other connections");
            return new Result<int>(-1, NO_CONTENT);
        }

        #endregion

        #region -> Shelf

        public async Task<IResult<IList<Shelf>>> GetAllShelves()
        {
            var shelves = await _wsContext.Shelves.ToListAsync();

            return new Result<IList<Shelf>>(shelves, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Shelf>> CreateShelf(Shelf request)
        {
            if (request == null)
            {
                _logger.LogDebug("CreateShelf: Shelf is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("CreateShelf: Shelf failed model validation");
                return null;
            }

            _wsContext.Shelves.Add(request);
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("CreateShelf: Successfully added shelf to database");
            }
            catch
            {
                _logger.LogError("CreateShelf: Error saving database on adding shelf");
                return null;
            }

            return new Result<Shelf>(request, OK);
        }

        public async Task<IResult<Shelf>> UpdateShelf(Shelf request)
        {
            if (request == null)
            {
                _logger.LogDebug("UpdateShelf: Shelf is null");
                return null;
            }
            if (!request.Validate())
            {
                _logger.LogDebug("UpdateShelf: Shelf failed model validation");
                return null;
            }

            var oldShelf = await _wsContext.Shelves.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
            if (oldShelf == null)
            {
                _logger.LogDebug($"UpdateShelf: No shelf exists with id ({request.Id})");
                return null;
            }

            oldShelf.BookcaseId = request.BookcaseId;
            oldShelf.Name = request.Name;
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateShelf: Successfully updated shelf in database");
            }
            catch
            {
                _logger.LogError($"UpdateShelf: Error saving database on updated shelf with id ({request.Id})");
                return null;
            }

            return new Result<Shelf>(oldShelf, OK);
        }

        public async Task<IResult<int>> DeleteShelf(int id)
        {
            var shelf = await _wsContext.Shelves.Include(x => x.Books).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (shelf == null)
            {
                _logger.LogDebug($"DeleteShelf: No shelf exists with id ({id})");
                return new Result<int>(-1, BAD_REQUEST);
            }

            // Delete shelf
            if (shelf.Books == null || shelf.Books.Count == 0)
            {
                _wsContext.Remove(shelf);
                try
                {
                    await _wsContext.SaveChangesAsync();
                }
                catch
                {
                    _logger.LogError($"DeleteShelf: Error saving database on deleting shelf with id ({id})");
                    return new Result<int>(-1, SERVER_ERROR);
                }

                _logger.LogDebug("DeleteShelf: Successfully deleted shelf from database");
                return new Result<int>(id, OK);
            }

            // Can't delete shelf because there is other connections at stake
            _logger.LogDebug($"DeleteShelf: Can't delete the shelf with id ({id}) as it has other connections");
            return new Result<int>(-1, NO_CONTENT);
        }

        #endregion

        #endregion

        #region -> Private Functions

        #region -> Author

        private async Task<bool> AddAuthorListAsync(IEnumerable<Author> authors)
        {
            if (authors == null || authors.Count() == 0)
            {
                _logger.LogDebug("AddAuthorListAsync: Author list is null or contains only authors that are already present in the database.");
                return true;
            }

            _wsContext.Authors.AddRange(authors);
            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("AddAuthorListAsync: Successfully added authors to database");
            }
            catch
            {
                _logger.LogError("AddAuthorListAsync: Error saving database on adding authors");
                return false;
            }

            return true;
        }

        private async Task<IList<BookAuthor>> UpdateBookAuthorListConnectionsAsync(IList<Author> newAuthors, IList<BookAuthor> bookAuthorsToDelete, Book book)
        {
            var bookAuthors = newAuthors.Select(x => new BookAuthor()
            {
                AuthorId = x.Id,
                BookId = book.Id
            }).ToList();

            _wsContext.BookHasAuthors.AddRange(bookAuthors);
            _wsContext.BookHasAuthors.RemoveRange(bookAuthorsToDelete);

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

            foreach (var bookAuthor in bookAuthors)
            {
                bookAuthor.Author = newAuthors.Where(x => x.Id == bookAuthor.AuthorId).Single();
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

            // Get the list of authors that need to be added and deleted from the BookAuthor list
            var addAuthors = authors.Where(x => !book.BookAuthors?.Any(y => y.AuthorId == x.Id) == true).ToList();
            var bookAuthorsToDelete = book.BookAuthors?.Where(x => !authors.Any(y => y.Id == x.AuthorId)).ToList();

            // Clone the book passed in and start building a new list of which books are going to be returned to the Client
            var currentBookAuthors = book.Clone().BookAuthors.Where(x => !bookAuthorsToDelete.Any(y => y.BookId == x.BookId && y.AuthorId == x.AuthorId)).ToList();

            var authorsToCreate = addAuthors.Where(x => x.Id <= 0);
            await AddAuthorListAsync(authorsToCreate);

            var newBookAuthors = await UpdateBookAuthorListConnectionsAsync(addAuthors, bookAuthorsToDelete, book);
            currentBookAuthors.AddRange(newBookAuthors);

            return currentBookAuthors;
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

        private async Task<IList<BookGenre>> UpdateBookGenreListConnectionsAsync(IList<Genre> newGenres, IList<BookGenre> bookGenresToDelete, Book book)
        {
            var bookGenres = newGenres.Select(x => new BookGenre()
            {
                BookId = book.Id,
                GenreId = x.Id
            }).ToList();

            _wsContext.BookHasGenres.AddRange(bookGenres);
            _wsContext.BookHasGenres.RemoveRange(bookGenresToDelete);

            try
            {
                await _wsContext.SaveChangesAsync();
                _logger.LogDebug("UpdateBookGenreListConnectionsAsync: Successfully made book author connections in the database");
            }
            catch
            {
                _logger.LogError("UpdateBookGenreListConnectionsAsync: Error saving database on book author connections");
                return null;
            }

            foreach (var bookGenre in bookGenres)
            {
                bookGenre.Genre = newGenres.Where(x => x.Id == bookGenre.GenreId).Single();
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

            // Get the list of genres that need to be added and deleted from the BookGenre list
            var addGenres = genres.Where(x => !book.BookGenres?.Any(y => y.GenreId == x.Id) == true).ToList();
            var bookGenresToDelete = book.BookGenres?.Where(x => !genres.Any(y => y.Id == x.GenreId)).ToList();

            // Clone the book passed in and start building a new list of which books are going to be returned to the Client
            var currentBookGenres = book.Clone().BookGenres.Where(x => !bookGenresToDelete.Any(y => y.BookId == x.BookId && y.GenreId == x.GenreId)).ToList();

            var newBookGenres = await UpdateBookGenreListConnectionsAsync(addGenres, bookGenresToDelete, book);
            currentBookGenres.AddRange(newBookGenres);

            return currentBookGenres;
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

        private async Task<Shelf> ShelfInBookCaseCheck (BookAddRequest request, Book book)
        {
            Shelf shelf = null;
            if (request.Bookcase != null && request.Shelf != null)
            {
                // New shelf, set Id to the bookcase Id
                if (request.Shelf.Id == 0)
                {
                    request.Shelf.BookcaseId = request.Bookcase.Id;
                }

                // Only add shelf to book if the shelf is on the correct bookcase, otherwise it's invalid somehow
                if (request.Shelf.BookcaseId == request.Bookcase.Id)
                {
                    return await UpdateShelfConnectionAsync(request.Shelf, book);
                }
            }

            return shelf;
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
