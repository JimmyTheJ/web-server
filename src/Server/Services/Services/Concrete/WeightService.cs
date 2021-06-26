using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Domain.Concrete;
using VueServer.Domain.Enums;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Context;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class WeightService : IWeightService
    {
        private ILogger _logger { get; set; }

        private IUserService _user { get; set; }

        private IWebHostEnvironment _env { get; set; }

        private readonly IWSContext _wsContext;

        public WeightService(ILoggerFactory logger, IUserService user, IWebHostEnvironment env, IWSContext wsContext)
        {
            _logger = logger?.CreateLogger<DirectoryService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _wsContext = wsContext ?? throw new ArgumentNullException("Webserver context is null");
        }

        #region -> Public Functions

        public async Task<IResult<IEnumerable<Weight>>> GetWeightList()
        {
            IEnumerable<Weight> weights;
            try
            {
                weights = await _wsContext.Weight.Where(x => x.UserId == _user.Id).OrderByDescending(x => x.Created).ToListAsync();
            }
            catch
            {
                _logger.LogWarning("WeightService.GetWeightList: Error getting list of weights");
                return new Result<IEnumerable<Weight>>(null, StatusCode.BAD_REQUEST);
            }

            return new Result<IEnumerable<Weight>>(weights, StatusCode.OK);
        }

        public async Task<IResult<Weight>> AddWeight(Weight weight)
        {
            if (weight == null)
            {
                _logger.LogWarning("WeightService.AddWeight: Weight is null");
                return new Result<Weight>(null, StatusCode.BAD_REQUEST);
            }

            weight.Id = 0;
            weight.UserId = _user.Id;
            _wsContext.Weight.Add(weight);

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("WeightService.AddWeight: Error saving changes");
                return new Result<Weight>(null, StatusCode.BAD_REQUEST);
            }

            return new Result<Weight>(weight, StatusCode.OK);
        }

        public async Task<IResult<Weight>> EditWeight(Weight weight)
        {
            if (weight == null)
            {
                _logger.LogInformation("WeightService.EditWeight: Weight is null");
                return new Result<Weight>(null, StatusCode.BAD_REQUEST);
            }

            // Get old weight, only allow weights from current user
            var oldWeight = await _wsContext.Weight.Where(x => x.Id == weight.Id && x.UserId == _user.Id).FirstOrDefaultAsync();
            if (oldWeight == null)
            {
                _logger.LogInformation("WeightService.EditWeight: No weight exists with the passed in Id for this user");
                return new Result<Weight>(null, StatusCode.BAD_REQUEST);
            }

            oldWeight.Created = weight.Created;
            oldWeight.Value = weight.Value;
            oldWeight.Notes = weight.Notes;

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("WeightService.EditWeight: Error saving changes");
                return new Result<Weight>(null, StatusCode.BAD_REQUEST);
            }

            return new Result<Weight>(weight, StatusCode.OK);
        }


        public async Task<IResult<bool>> DeleteWeight(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("WeightService.DeleteWeight: Id is <= 0");
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            var weight = await _wsContext.Weight.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (weight == null)
            {
                _logger.LogWarning($"WeightService.DeleteWeight: No weight exists with id: {id}");
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }
            _wsContext.Weight.Remove(weight);

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("[WeightService.DeleteWeight]: Error saving changes");
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            return new Result<bool>(true, StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        #endregion
    }
}
