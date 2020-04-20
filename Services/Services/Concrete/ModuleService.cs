﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Concrete;
using VueServer.Domain.Interface;
using VueServer.Models.Context;
using VueServer.Models.Modules;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class ModuleService : IModuleService
    {
        /// <summary>Hosting environment</summary>
        private readonly IWebHostEnvironment _env;
        /// <summary> Configuration file. </summary>
        private readonly IConfiguration _config;
        /// <summary>Logger</summary>
        private readonly ILogger _logger;
        /// <summary>User Context (Database)</summary>
        private readonly IWSContext _context;
        /// <summary>User service to manipulate the context using the user manager</summary>
        private readonly IUserService _user;

        public ModuleService(
            IWSContext context,
            IWebHostEnvironment env,
            ILoggerFactory logger,
            IUserService user
        )
        {
            _env = env ?? throw new ArgumentNullException("Hosting environment is null");
            _user = user ?? throw new ArgumentNullException("User service is null");
            _context = context ?? throw new ArgumentNullException("User context is null");
            _logger = logger?.CreateLogger<ModuleService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        public async Task<IResult<IEnumerable<ModuleAddOn>>> GetActiveModulesForUser()
        {
            var userModules = await _context.UserHasModule
                .Include(x => x.ModuleAddOn)
                .Where(x => x.UserId == _user.Name)
                .Select(x => x.ModuleAddOn)
                .ToListAsync();

            return new Result<IEnumerable<ModuleAddOn>>(userModules, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ModuleAddOn>>> GetAllModules()
        {
            var userModules = await _context.Modules.ToListAsync();

            return new Result<IEnumerable<ModuleAddOn>>(userModules, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<UserHasModuleAddOn>>> GetActiveModulesForAllUsers()
        {
            var userModules = await _context.UserHasModule
                .Include(x => x.ModuleAddOn)
                .Include(x => x.User)
                .OrderBy(x => x.UserId)
                .ToListAsync();

            return new Result<IEnumerable<UserHasModuleAddOn>>(userModules, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<UserHasModuleAddOn>> AddModuleToUser(UserHasModuleAddOn userModule)
        {
            if (userModule == null)
            {
                _logger.LogInformation("[ModuleService] AddModuleToUser: User Module is null");
                return new Result<UserHasModuleAddOn>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userModule.ModuleAddOnId))
            {
                _logger.LogInformation("[ModuleService] AddModuleToUser: Module to add is null");
                return new Result<UserHasModuleAddOn>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userModule.UserId))
            {
                _logger.LogInformation("[ModuleService] AddModuleToUser: User to add is null");
                return new Result<UserHasModuleAddOn>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var newUserModule = new UserHasModuleAddOn() { UserId = userModule.UserId, ModuleAddOnId = userModule.ModuleAddOnId };
            _context.UserHasModule.Add(newUserModule);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                _logger.LogWarning("[ModuleService] AddModuleToUser: Error saving database");
            }            

            return new Result<UserHasModuleAddOn>(newUserModule, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteModuleFromUser(UserHasModuleAddOn userModule)
        {
            if (userModule == null)
            {
                _logger.LogInformation("[ModuleService] DeleteModuleFromUser: User Module is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userModule.ModuleAddOnId))
            {
                _logger.LogInformation("[ModuleService] DeleteModuleFromUser: Module to delete is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userModule.UserId))
            {
                _logger.LogInformation("[ModuleService] DeleteModuleFromUser: User to delete is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var dbUserModule = _context.UserHasModule.Where(x => x.UserId == userModule.UserId && x.ModuleAddOnId == userModule.ModuleAddOnId).FirstOrDefault();
            if (dbUserModule == null)
            {
                _logger.LogInformation("[ModuleService] DeleteModuleFromUser: UserModule doesn't exist in database");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            _context.UserHasModule.Remove(dbUserModule);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                _logger.LogWarning("[ModuleService] DeleteModuleFromUser: Error saving database");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }
    }
}