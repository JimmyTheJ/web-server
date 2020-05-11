using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using VueServer.Domain.Enums;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Directory;


namespace VueServer.Services.Interface
{
    public interface IWeightService
    {
        Task<IResult<IEnumerable<Weight>>> GetWeightList ();

        Task<IResult<Weight>> AddWeight (Weight weight);

        Task<IResult<Weight>> EditWeight(Weight weight);

        Task<IResult<bool>> DeleteWeight (int id);
    }
}
