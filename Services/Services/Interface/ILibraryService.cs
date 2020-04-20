using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Concrete;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Library;
using VueServer.Models.Request;

namespace VueServer.Services.Interface
{
    public interface ILibraryService
    {
        #region -> Book

        Task<IResult<IList<Book>>> GetAllBooks();
        Task<IResult<Book>> GetBook(int id);
        Task<IResult<Book>> CreateBook(BookAddRequest request);
        Task<IResult<Book>> UpdateBook(BookAddRequest request);
        Task<IResult<int>> DeleteBook (int id);

        #endregion

        #region -> Author

        Task<IResult<IList<Author>>> GetAllAuthors();
        Task<IResult<Author>> CreateAuthor(Author request);
        Task<IResult<Author>> UpdateAuthor(Author request);
        Task<IResult<int>> DeleteAuthor(int id);

        #endregion

        #region -> Bookcase

        Task<IResult<IList<Bookcase>>> GetAllBookcases();
        Task<IResult<Bookcase>> CreateBookcase(Bookcase request);
        Task<IResult<Bookcase>> UpdateBookcase(Bookcase request);
        Task<IResult<int>> DeleteBookcase(int id);
        #endregion

        #region -> Genre

        Task<IResult<IList<Genre>>> GetAllGenres();
        Task<IResult<Genre>> CreateGenre(Genre request);
        Task<IResult<Genre>> UpdateGenre(Genre request);
        Task<IResult<int>> DeleteGenre(int id);

        #endregion

        #region -> Series

        Task<IResult<IList<Series>>> GetAllSeries();
        Task<IResult<Series>> CreateSeries(Series request);
        Task<IResult<Series>> UpdateSeries(Series request);
        Task<IResult<int>> DeleteSeries(int id);

        #endregion

        #region -> Shelf

        Task<IResult<IList<Shelf>>> GetAllShelves();
        Task<IResult<Shelf>> CreateShelf(Shelf request);
        Task<IResult<Shelf>> UpdateShelf(Shelf request);
        Task<IResult<int>> DeleteShelf(int id);

        #endregion
    }
}
