using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core.Cache;
using VueServer.Core.Objects;
using VueServer.Domain;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Context;
using VueServer.Models.Directory;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class AdminService : IAdminService
    {
        /// <summary>Logger</summary>
        private readonly ILogger _logger;
        /// <summary>User Context (Database)</summary>
        private readonly IWSContext _context;
        /// <summary>User service to manipulate the context using the user manager</summary>
        private readonly IUserService _user;
        /// <summary>Custom Caching service</summary>
        private readonly IVueServerCache _serverCache;

        public AdminService(IWSContext context, ILoggerFactory logger, IUserService user, IVueServerCache serverCache)
        {
            _user = user ?? throw new ArgumentNullException("User service is null");
            _context = context ?? throw new ArgumentNullException("User context is null");
            _serverCache = serverCache ?? throw new ArgumentNullException("Server cache is null");
            _logger = logger?.CreateLogger<AccountService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        public async Task<IResult<IEnumerable<ServerSettings>>> GetDirectorySettings()
        {
            var directorySettings = await _context.ServerSettings.Where(x => x.Key.StartsWith(DomainConstants.ServerSettings.BaseKeys.Directory)).ToListAsync();
            return new Result<IEnumerable<ServerSettings>>(directorySettings, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> SetServerSetting(ServerSettings setting)
        {
            var dbSetting = await _context.ServerSettings.Where(x => x.Key == setting.Key).FirstOrDefaultAsync();
            if (dbSetting == null)
            {
                dbSetting = new ServerSettings()
                {
                    Key = setting.Key,
                    Value = setting.Value
                };

                _context.ServerSettings.Add(dbSetting);
            }
            else
            {
                if (dbSetting.Value == setting.Value)
                {
                    _logger.LogDebug($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Value matches. No need to save database with changes, returning true but doing nothing.");
                    return new Result<bool>(true, Domain.Enums.StatusCode.OK);
                }

                dbSetting.Value = setting.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving after adding or editing a {nameof(ServerSettings)} with key: {setting.Key}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<bool>> DeleteServerSetting(string key)
        {
            var setting = _context.ServerSettings.Where(x => x.Key == key).FirstOrDefault();
            if (setting == null)
            {
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }

            try
            {
                _context.Remove(setting);
                await _context.SaveChangesAsync();
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving after deleting a {nameof(ServerSettings)} with key: {key}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<IEnumerable<ServerGroupDirectory>>> GetGroupDirectories()
        {
            var directories = await _context.ServerGroupDirectory.ToListAsync();
            return new Result<IEnumerable<ServerGroupDirectory>>(directories, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ServerUserDirectory>>> GetUserDirectories()
        {
            var directories = await _context.ServerUserDirectory.ToListAsync();
            return new Result<IEnumerable<ServerUserDirectory>>(directories, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<int>> AddGroupDirectory(ServerGroupDirectory dir)
        {
            if (dir == null || dir.Id != 0)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Directory must exist and have an Id of 0 to be able to be created");
                return new Result<int>(0, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            _context.ServerGroupDirectory.Add(dir);
            try
            {
                await _context.SaveChangesAsync();
                return new Result<int>(dir.Id, Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving after adding {nameof(ServerGroupDirectory)} with name: {dir.Name}");
                return new Result<int>(0, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<long>> AddUserDirectory(ServerUserDirectory dir)
        {
            if (dir == null || dir.Id != 0)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Directory must exist and have an Id of 0 to be able to be created");
                return new Result<long>(0, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            _context.ServerUserDirectory.Add(dir);
            try
            {
                await _context.SaveChangesAsync();
                return new Result<long>(dir.Id, Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving after adding {nameof(ServerUserDirectory)} with name: {dir.Name}");
                return new Result<long>(0, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<bool>> DeleteGroupDirectory(int id)
        {
            if (id < 1)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Id must be > 0 to be valid");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var item = await _context.ServerGroupDirectory.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (item == null)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: No {nameof(ServerGroupDirectory)} found with Id: {id}");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            _context.ServerGroupDirectory.Remove(item);
            try
            {
                await _context.SaveChangesAsync();
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving after deleting {nameof(ServerGroupDirectory)} with id: {id}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<bool>> DeleteUserDirectory(long id)
        {
            if (id < 1)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Id must be > 0 to be valid");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var item = await _context.ServerUserDirectory.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (item == null)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: No {nameof(ServerUserDirectory)} found with Id: {id}");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            _context.ServerUserDirectory.Remove(item);
            try
            {
                await _context.SaveChangesAsync();
                return new Result<bool>(true, Domain.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving after deleting {nameof(ServerUserDirectory)} with id: {id}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }
    }
}
