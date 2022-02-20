using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core.Objects;
using VueServer.Domain.Enums;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Context;
using VueServer.Services.User;

namespace VueServer.Services.Weight
{
    public class WeightService : IWeightService
    {
        private ILogger _logger { get; set; }

        private IUserService _user { get; set; }

        private IWebHostEnvironment _env { get; set; }

        private readonly IWSContext _wsContext;

        public WeightService(ILoggerFactory logger, IUserService user, IWebHostEnvironment env, IWSContext wsContext)
        {
            _logger = logger?.CreateLogger<WeightService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _wsContext = wsContext ?? throw new ArgumentNullException("Webserver context is null");
        }

        #region -> Public Functions

        public async Task<IResult<IEnumerable<Weights>>> GetWeightList()
        {
            IEnumerable<Weights> weights;
            try
            {
                weights = await _wsContext.Weight.Where(x => x.UserId == _user.Id).OrderByDescending(x => x.Created).ToListAsync();
            }
            catch
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(GetWeightList)}: Error getting list of weights");
                return new Result<IEnumerable<Weights>>(null, StatusCode.BAD_REQUEST);
            }

            return new Result<IEnumerable<Weights>>(weights, StatusCode.OK);
        }

        public async Task<IResult<Weights>> AddWeight(Weights weight)
        {
            if (weight == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(AddWeight)}: Weight is null");
                return new Result<Weights>(null, StatusCode.BAD_REQUEST);
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
                _logger.LogError($"[{this.GetType().Name}] {nameof(AddWeight)}: Error saving changes");
                return new Result<Weights>(null, StatusCode.BAD_REQUEST);
            }

            return new Result<Weights>(weight, StatusCode.OK);
        }

        public async Task<IResult<Weights>> EditWeight(Weights weight)
        {
            if (weight == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(EditWeight)}: Weight is null");
                return new Result<Weights>(null, StatusCode.BAD_REQUEST);
            }

            // Get old weight, only allow weights from current user
            var oldWeight = await _wsContext.Weight.Where(x => x.Id == weight.Id && x.UserId == _user.Id).FirstOrDefaultAsync();
            if (oldWeight == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(EditWeight)}: No weight exists with the passed in Id for this user");
                return new Result<Weights>(null, StatusCode.BAD_REQUEST);
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
                _logger.LogError($"[{this.GetType().Name}] {nameof(EditWeight)}: Error saving changes");
                return new Result<Weights>(null, StatusCode.BAD_REQUEST);
            }

            return new Result<Weights>(weight, StatusCode.OK);
        }


        public async Task<IResult<bool>> DeleteWeight(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(DeleteWeight)}: Id is <= 0");
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            var weight = await _wsContext.Weight.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (weight == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(DeleteWeight)}: No weight exists with id: {id}");
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }
            _wsContext.Weight.Remove(weight);

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(DeleteWeight)}: Error saving changes");
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            return new Result<bool>(true, StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        #endregion
    }
}
