using Microsoft.AspNetCore.Hosting;
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
        /// <summary>Logger</summary>
        private readonly ILogger _logger;
        /// <summary>User Context (Database)</summary>
        private readonly IWSContext _context;
        /// <summary>User service to manipulate the context using the user manager</summary>
        private readonly IUserService _user;

        public ModuleService(
            IWSContext context,
            ILoggerFactory logger,
            IUserService user
        )
        {
            _user = user ?? throw new ArgumentNullException("User service is null");
            _context = context ?? throw new ArgumentNullException("User context is null");
            _logger = logger?.CreateLogger<ModuleService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        public async Task<IResult<IEnumerable<ModuleAddOn>>> GetActiveModulesForUser()
        {
            var userModules = await _context.UserHasModule
                .Include(x => x.ModuleAddOn)
                .Where(x => x.UserId == _user.Id)
                .Select(x => new ModuleAddOn()
                {
                    Id = x.ModuleAddOn.Id,
                    Name = x.ModuleAddOn.Name,
                    UserModuleFeatures = _context.UserHasFeature
                        .Include(y => y.ModuleFeature)
                        .Include(y => y.User)
                        .Where(y => y.ModuleFeature.ModuleAddOnId == x.ModuleAddOnId && y.UserId == _user.Id)
                        .ToList()
                }).ToListAsync();

            return new Result<IEnumerable<ModuleAddOn>>(userModules, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ModuleAddOn>>> GetAllModules()
        {
            var userModules = await _context.Modules.Include(x => x.Features).ToListAsync();

            return new Result<IEnumerable<ModuleAddOn>>(userModules, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IDictionary<string, IList<ModuleAddOn>>>> GetActiveModulesForAllUsers()
        {
            var userModules = await _context.UserHasModule
                .Include(x => x.ModuleAddOn)
                .Include(x => x.User)
                .OrderBy(x => x.UserId)
                .Select(x => new UserHasModuleAddOn()
                {
                    ModuleAddOn = new ModuleAddOn()
                    {
                        Id = x.ModuleAddOn.Id,
                        Name = x.ModuleAddOn.Name,
                        UserModuleFeatures = _context.UserHasFeature
                            .Include(y => y.ModuleFeature)
                            .Include(y => y.User)
                            .Where(y => y.ModuleFeature.ModuleAddOnId == x.ModuleAddOnId && y.UserId == x.UserId)
                            .ToList()
                    },
                    ModuleAddOnId = x.ModuleAddOnId,
                    User = x.User,
                    UserId = x.UserId
                }).ToListAsync();

            var userModuleGroups = new Dictionary<string, IList<ModuleAddOn>>();
            foreach (var module in userModules) 
            {
                if (!userModuleGroups.ContainsKey(module.UserId))
                {
                    userModuleGroups[module.UserId] = new List<ModuleAddOn>();
                }

                if (module.ModuleAddOn.UserModuleFeatures != null)
                {
                    if (module.ModuleAddOn.Features == null)
                    {
                        module.ModuleAddOn.Features = new List<ModuleFeature>();
                    }

                    foreach (var feature in module.ModuleAddOn.UserModuleFeatures)
                    {
                        feature.ModuleFeature.UserModuleFeatures = null;
                        module.ModuleAddOn.Features.Add(feature.ModuleFeature);
                    }
                }

                module.ModuleAddOn.UserModuleFeatures = null;
                userModuleGroups[module.UserId].Add(module.ModuleAddOn);
            }

            return new Result<IDictionary<string, IList<ModuleAddOn>>>(userModuleGroups, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<UserHasModuleAddOn>>> GetModulesAndFeaturesForOtherUser(string user)
        {
            var userModules = await _context.UserHasModule
                .Include(x => x.ModuleAddOn)
                .Include(x => x.User)
                .OrderBy(x => x.UserId)
                .Where(x => x.UserId == user)
                .Select(x => new UserHasModuleAddOn()
                {
                    ModuleAddOn = new ModuleAddOn()
                    {
                        Id = x.ModuleAddOn.Id,
                        Name = x.ModuleAddOn.Name,
                        UserModuleFeatures = _context.UserHasFeature
                            .Include(y => y.ModuleFeature)
                            .Include(y => y.User)
                            .Where(y => y.ModuleFeature.ModuleAddOnId == x.ModuleAddOnId && y.UserId == x.UserId)
                            .ToList()
                    },
                    ModuleAddOnId = x.ModuleAddOnId,
                    User = x.User,
                    UserId = x.UserId
                }).ToListAsync();

            return new Result<IEnumerable<UserHasModuleAddOn>>(userModules, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> AddModuleToUser(UserHasModuleAddOn userModule)
        {
            if (userModule == null)
            {
                _logger.LogInformation("[ModuleService] AddModuleToUser: User Module is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userModule.ModuleAddOnId))
            {
                _logger.LogInformation("[ModuleService] AddModuleToUser: Module to add is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userModule.UserId))
            {
                _logger.LogInformation("[ModuleService] AddModuleToUser: User to add is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
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
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
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

            var dbUserModule = await _context.UserHasModule.Include(x => x.ModuleAddOn).ThenInclude(x => x.Features)
                .Where(x => x.UserId == userModule.UserId && x.ModuleAddOnId == userModule.ModuleAddOnId).FirstOrDefaultAsync();
            if (dbUserModule == null)
            {
                _logger.LogInformation("[ModuleService] DeleteModuleFromUser: UserModule doesn't exist in database");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var dbUserModuleFeatures = _context.UserHasFeature.Include(x => x.ModuleFeature).AsEnumerable()
                .Where(x => x.UserId == userModule.UserId && dbUserModule.ModuleAddOn.Features.Contains(x.ModuleFeature));
            if (dbUserModuleFeatures.Count() > 0)
            {
                _context.UserHasFeature.RemoveRange(dbUserModuleFeatures);
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

        public async Task<IResult<bool>> AddFeatureToUser(UserHasModuleFeature userFeature)
        {
            if (userFeature == null)
            {
                _logger.LogInformation("[ModuleService] AddFeatureToUser: User Feature is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userFeature.ModuleFeatureId))
            {
                _logger.LogInformation("[ModuleService] AddFeatureToUser: Feature to add is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userFeature.UserId))
            {
                _logger.LogInformation("[ModuleService] AddFeatureToUser: User to add is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var newUserFeature = new UserHasModuleFeature() { UserId = userFeature.UserId, ModuleFeatureId = userFeature.ModuleFeatureId };
            _context.UserHasFeature.Add(newUserFeature);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                _logger.LogWarning("[ModuleService] AddFeatureToUser: Error saving database");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }
        public async Task<IResult<bool>> DeleteFeatureFromUser(UserHasModuleFeature userFeature)
        {
            if (userFeature == null)
            {
                _logger.LogInformation("[ModuleService] DeleteFeatureFromUser: User Feature is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userFeature.ModuleFeatureId))
            {
                _logger.LogInformation("[ModuleService] DeleteFeatureFromUser: Feature to delete is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(userFeature.UserId))
            {
                _logger.LogInformation("[ModuleService] DeleteFeatureFromUser: User to delete is null");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var dbUserFeature = _context.UserHasFeature.Where(x => x.UserId == userFeature.UserId && x.ModuleFeatureId == userFeature.ModuleFeatureId).FirstOrDefault();
            if (dbUserFeature == null)
            {
                _logger.LogInformation("[ModuleService] DeleteFeatureFromUser: UserFeature doesn't exist in database");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            _context.UserHasFeature.Remove(dbUserFeature);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                _logger.LogWarning("[ModuleService] DeleteFeatureFromUser: Error saving database");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DoesUserHaveModule(string user, string module)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(module))
            {
                _logger.LogInformation($"[ModuleService] DoesUserHaveModule: user or module value is null (user: {user} / module: {module})");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var hasModule = await _context.UserHasModule.Where(x => x.UserId == user && x.ModuleAddOnId == module).SingleOrDefaultAsync();
            if (hasModule != null)
            {
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            else
            {
                return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
            }
        }

        public async Task<IResult<bool>> DoesUserHaveFeature(string user, string feature)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(feature))
            {
                _logger.LogInformation($"[ModuleService] DoesUserHaveModule: user or feature value is null (user: {user} / feature: {feature})");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var hasModule = await _context.UserHasFeature.Where(x => x.UserId == user && x.ModuleFeatureId == feature).SingleOrDefaultAsync();
            if (hasModule != null)
            {
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            else
            {
                return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
            }
        }
    }
}
