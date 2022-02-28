using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core.Objects;
using VueServer.Domain;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Cache;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Core.Services.User;

namespace VueServer.Modules.Core.Services.Admin
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
            _logger = logger?.CreateLogger<AdminService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        public async Task<IResult<IEnumerable<ServerSettings>>> GetDirectorySettings()
        {
            var directorySettings = await _context.ServerSettings.Where(x => x.Key.StartsWith(DomainConstants.ServerSettings.BaseKeys.Directory)).ToListAsync();
            return new Result<IEnumerable<ServerSettings>>(directorySettings, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> SetServerSetting(ServerSettings setting)
        {
            if (setting == null || string.IsNullOrWhiteSpace(setting.Key))
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(SetServerSetting)}: Setting is null or key is empty.");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(setting.Value))
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(SetServerSetting)}: Setting value is empty.");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

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
                    _logger.LogDebug($"[{this.GetType().Name}] {nameof(SetServerSetting)}: Value matches. No need to save database with changes, returning true but doing nothing.");
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
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(SetServerSetting)}: Error saving after adding or editing a {nameof(ServerSettings)} with key: {setting.Key}");
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
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(DeleteServerSetting)}: Error saving after deleting a {nameof(ServerSettings)} with key: {key}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

    }
}
