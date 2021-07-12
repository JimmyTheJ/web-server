using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Models;


namespace VueServer.Services.Interface
{
    public interface IWeightService
    {
        Task<IResult<IEnumerable<Weight>>> GetWeightList();

        Task<IResult<Weight>> AddWeight(Weight weight);

        Task<IResult<Weight>> EditWeight(Weight weight);

        Task<IResult<bool>> DeleteWeight(int id);
    }
}
