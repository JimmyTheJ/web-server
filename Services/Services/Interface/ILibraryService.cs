using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Concrete;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Models.Library;
using VueServer.Models.Models.Request;

namespace VueServer.Services.Interface
{
    public interface ILibraryService
    {
        Task<IResult<IList<Book>>> GetAllBooks();

        Task<IResult<Book>> GetBook(int id);

        Task<IResult<Book>> CreateBook(BookAddRequest request);

        Task<IResult<Book>> UpdateBook(BookAddRequest request);

        Task<IResult> DeleteBook (int id);

        Task<IResult<IList<Genre>>> GetAllGenres();

        Task<IResult<IList<Author>>> GetAllAuthors();

        Task<IResult<IList<Series>>> GetAllSeries();

        Task<IResult<IList<Bookshelf>>> GetAllBookshelves();
    }
}
