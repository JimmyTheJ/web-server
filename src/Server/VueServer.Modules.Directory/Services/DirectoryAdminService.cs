using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core.Objects;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Cache;
using VueServer.Modules.Core.Services.User;
using VueServer.Modules.Directory.Context;
using VueServer.Modules.Directory.Models;

namespace VueServer.Modules.Directory.Services
{
    public class DirectoryAdminService : IDirectoryAdminService
    {
        /// <summary>Logger</summary>
        private readonly ILogger _logger;
        /// <summary>Directory Context (Database)</summary>
        private readonly IDirectoryContext _context;
        /// <summary>User service to manipulate the context using the user manager</summary>
        private readonly IUserService _user;
        /// <summary>Custom Caching service</summary>
        private readonly IVueServerCache _serverCache;

        public DirectoryAdminService(IDirectoryContext context, ILoggerFactory logger, IUserService user, IVueServerCache serverCache)
        {
            _user = user ?? throw new ArgumentNullException("User service is null");
            _context = context ?? throw new ArgumentNullException("Directory context is null");
            _serverCache = serverCache ?? throw new ArgumentNullException("Server cache is null");
            _logger = logger?.CreateLogger<DirectoryAdminService>() ?? throw new ArgumentNullException("Logger factory is null");
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
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(AddGroupDirectory)}: Directory must exist and have an Id of 0 to be able to be created");
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
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(AddGroupDirectory)}: Error saving after adding {nameof(ServerGroupDirectory)} with name: {dir.Name}");
                return new Result<int>(0, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<long>> AddUserDirectory(ServerUserDirectory dir)
        {
            if (dir == null || dir.Id != 0)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(AddUserDirectory)}: Directory must exist and have an Id of 0 to be able to be created");
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
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(AddUserDirectory)}: Error saving after adding {nameof(ServerUserDirectory)} with name: {dir.Name}");
                return new Result<long>(0, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<bool>> DeleteGroupDirectory(int id)
        {
            if (id < 1)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(DeleteGroupDirectory)}: Id must be > 0 to be valid");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var item = await _context.ServerGroupDirectory.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (item == null)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(DeleteGroupDirectory)}: No {nameof(ServerGroupDirectory)} found with Id: {id}");
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
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(DeleteGroupDirectory)}: Error saving after deleting {nameof(ServerGroupDirectory)} with id: {id}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<bool>> DeleteUserDirectory(long id)
        {
            if (id < 1)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(DeleteUserDirectory)}: Id must be > 0 to be valid");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var item = await _context.ServerUserDirectory.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (item == null)
            {
                _logger.LogDebug($"[{this.GetType().Name}] {nameof(DeleteUserDirectory)}: No {nameof(ServerUserDirectory)} found with Id: {id}");
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
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(DeleteUserDirectory)}: Error saving after deleting {nameof(ServerUserDirectory)} with id: {id}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }
        }
    }
}
