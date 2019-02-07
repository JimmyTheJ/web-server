﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using VueServer.Domain;
using VueServer.Domain.Enums;
using VueServer.Domain.Helper;
using VueServer.Domain.Interface;
using VueServer.Domain.Concrete;
using VueServer.Services.Interface;

using VueServer.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VueServer.Models.Directory;
using System.Collections;
using VueServer.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VueServer.Services.Concrete
{
    public class WeightService : IWeightService
    {
        private ILogger _logger { get; set; }

        private IUserService _user { get; set; }

        private IWebHostEnvironment _env { get; set; }
                
        private readonly WSContext _wsContext;

        public WeightService(ILoggerFactory logger, IUserService user, IWebHostEnvironment env, WSContext wsContext)
        {
            _logger = logger?.CreateLogger<DirectoryService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _wsContext = wsContext ?? throw new ArgumentNullException("Webserver context is null");
        }

        # region -> Public Functions
        
        public async Task<IResult<IEnumerable<Weight>>> GetWeightList()
        {
            IEnumerable<Weight> weights;
            try
            {
                weights = await _wsContext.Weight.Where(x => x.UserId == _user.Name).OrderByDescending(x => x.Created).ToListAsync();
            }
            catch
            {
                _logger.LogWarning("WeightService.GetWeightList: Error getting list of weights");
                return new Result<IEnumerable<Weight>>(null, StatusCode.BAD_REQUEST);
            }
            
            return new Result<IEnumerable<Weight>>(weights, StatusCode.OK);
        }

        public async Task<IResult<Weight>> AddWeight (Weight weight)
        {
            if (weight == null)
            {
                _logger.LogWarning("WeightService.AddWeight: Weight is null");
                return new Result<Weight>(null, StatusCode.BAD_REQUEST);
            }

            weight.Id = 0;
            weight.UserId = _user.Name;
            _wsContext.Weight.Add(weight);

            try
            {
                await _wsContext.SaveChangesAsync();
            }
            catch(Exception)
            {
                _logger.LogError("[WeightService.AddWeight] Create: Error saving changes");
                return new Result<Weight>(null, StatusCode.BAD_REQUEST);
            }

            return new Result<Weight>(weight, StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteWeight (int id)
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
            catch(Exception)
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