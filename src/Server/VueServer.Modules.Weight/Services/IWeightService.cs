using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Weight.Models;

namespace VueServer.Modules.Weight.Services
{
    public interface IWeightService
    {
        Task<IResult<IEnumerable<Weights>>> GetWeightList();

        Task<IResult<Weights>> AddWeight(Weights weight);

        Task<IResult<Weights>> EditWeight(Weights weight);

        Task<IResult<bool>> DeleteWeight(int id);
    }
}
