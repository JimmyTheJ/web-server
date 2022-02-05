using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Models;

namespace VueServer.Services.Note
{
    public interface INoteService
    {
        Task<IResult<List<Notes>>> GetAll();

        Task<IResult<List<Notes>>> Get();

        Task<IResult<Notes>> Create(Notes note);

        Task<IResult<Notes>> Update(Notes note);

        Task<IResult> Delete(int id);

    }
}
