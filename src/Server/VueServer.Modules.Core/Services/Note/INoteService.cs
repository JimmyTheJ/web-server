using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Models;

namespace VueServer.Modules.Core.Services.Note
{
    public interface INoteService
    {
        Task<IServerResult<List<Notes>>> GetAll();

        Task<IServerResult<List<Notes>>> Get();

        Task<IServerResult<Notes>> Create(Notes note);

        Task<IServerResult<Notes>> Update(Notes note);

        Task<IServerResult> Delete(int id);

    }
}
