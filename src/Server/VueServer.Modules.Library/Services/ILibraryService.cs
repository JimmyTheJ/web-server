using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Library.Models;
using VueServer.Modules.Library.Models.Request;

namespace VueServer.Modules.Library.Services
{
    public interface ILibraryService
    {
        #region -> Book

        Task<IServerResult<IList<Book>>> GetAllBooks();
        Task<IServerResult<Book>> GetBook(int id);
        Task<IServerResult<Book>> CreateBook(BookAddRequest request);
        Task<IServerResult<Book>> UpdateBook(BookAddRequest request);
        Task<IServerResult<int>> DeleteBook(int id);

        #endregion

        #region -> Author

        Task<IServerResult<IList<Author>>> GetAllAuthors();
        Task<IServerResult<Author>> CreateAuthor(Author request);
        Task<IServerResult<Author>> UpdateAuthor(Author request);
        Task<IServerResult<int>> DeleteAuthor(int id);

        #endregion

        #region -> Bookcase

        Task<IServerResult<IList<Bookcase>>> GetAllBookcases();
        Task<IServerResult<Bookcase>> CreateBookcase(Bookcase request);
        Task<IServerResult<Bookcase>> UpdateBookcase(Bookcase request);
        Task<IServerResult<int>> DeleteBookcase(int id);
        #endregion

        #region -> Genre

        Task<IServerResult<IList<Genre>>> GetAllGenres();

        #endregion

        #region -> Series

        Task<IServerResult<IList<Series>>> GetAllSeries();
        Task<IServerResult<Series>> CreateSeries(Series request);
        Task<IServerResult<Series>> UpdateSeries(Series request);
        Task<IServerResult<int>> DeleteSeries(int id);

        #endregion

        #region -> Shelf

        Task<IServerResult<IList<Shelf>>> GetAllShelves();
        Task<IServerResult<Shelf>> CreateShelf(Shelf request);
        Task<IServerResult<Shelf>> UpdateShelf(Shelf request);
        Task<IServerResult<int>> DeleteShelf(int id);

        #endregion
    }
}
