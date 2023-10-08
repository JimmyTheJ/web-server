using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Weight.Models;

namespace VueServer.Modules.Weight.Services
{
    public interface IWeightService
    {
        Task<IServerResult<IEnumerable<Weights>>> GetWeightList();

        Task<IServerResult<Weights>> AddWeight(Weights weight);

        Task<IServerResult<Weights>> EditWeight(Weights weight);

        Task<IServerResult<bool>> DeleteWeight(int id);
    }
}
