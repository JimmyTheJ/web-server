using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core;
using VueServer.Core.Helper;
using VueServer.Core.Objects;
using VueServer.Domain;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Cache;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Core.Models.User;
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

        public async Task<IServerResult<IEnumerable<ServerSettings>>> GetDirectorySettings()
        {
            var directorySettings = await _context.ServerSettings.Where(x => x.Key.StartsWith(DomainConstants.ServerSettings.BaseKeys.Directory)).ToListAsync();
            return new Result<IEnumerable<ServerSettings>>(directorySettings, Domain.Enums.StatusCode.OK);
        }

        public async Task<IServerResult<IEnumerable<ServerGroupDirectory>>> GetGroupDirectories()
        {
            var directories = await _context.ServerGroupDirectory.ToListAsync();
            return new Result<IEnumerable<ServerGroupDirectory>>(directories, Domain.Enums.StatusCode.OK);
        }

        public async Task<IServerResult<IEnumerable<ServerUserDirectory>>> GetUserDirectories()
        {
            var directories = await _context.ServerUserDirectory.ToListAsync();
            return new Result<IEnumerable<ServerUserDirectory>>(directories, Domain.Enums.StatusCode.OK);
        }

        public async Task<IServerResult<int>> AddGroupDirectory(ServerGroupDirectory dir)
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

        public async Task<IServerResult<long>> AddUserDirectory(ServerUserDirectory dir)
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

        public async Task<IServerResult<bool>> DeleteGroupDirectory(int id)
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

        public async Task<IServerResult<bool>> DeleteUserDirectory(long id)
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

        public async Task<IServerResult<bool>> CreateDefaultFolder(string username)
        {
            if (username == null)
            {
                _logger.LogTrace($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: Username provided was null. Cannot create default folder");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var user = await _user.GetUserByIdAsync(username.ToLower());
            if (user == null)
            {
                _logger.LogTrace($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: User does not exist in database. Cannot create default folder");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var directorySettings = await _context.ServerSettings.Where(x => x.Key.StartsWith(DomainConstants.ServerSettings.BaseKeys.Directory)).ToListAsync();
            if (directorySettings == null || directorySettings.Count == 0)
            {
                _logger.LogTrace($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: No directory settings exist. Will not create default folder");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var shouldCreate = directorySettings.Where(x => x.Key == DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.ShouldUseDefaultPath).Select(x => x.Value?.StringToBool() ?? null).FirstOrDefault();
            if (!shouldCreate.HasValue || (shouldCreate.HasValue && shouldCreate.Value == false))
            {
                _logger.LogTrace($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: No {DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.ShouldUseDefaultPath} setting set. Will not create default folder");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var createPath = directorySettings.Where(x => x.Key == DomainConstants.ServerSettings.BaseKeys.Directory + DomainConstants.ServerSettings.Directory.DefaultPathValue).Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(createPath))
            {
                _logger.LogTrace($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: Default folder setting exists, but is null or empty. Will not create default folder");
                return new Result<bool>(false, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var argError = false;
            string arg = null;

            // TODO: Support multiple parameters
            for (int i = 0; i < createPath.Length; i++)
            {
                if (createPath[i] == '{')
                {
                    int start = i;
                    while (true)
                    {
                        if (createPath[i] == '}')
                        {
                            arg = createPath.Substring(start, i - start + 1).ToLower();
                            break;
                        }

                        if (i == createPath.Length)
                        {
                            argError = true;
                            break;
                        }

                        i++;
                    }
                }

                if (argError)
                {
                    break;
                }
            }

            if (argError)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: Error with an argument used");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            if (arg != null)
            {
                if (arg.Contains(nameof(WSUser.Id).ToLower()))
                {
                    createPath = createPath.Replace(arg, user.Id, StringComparison.OrdinalIgnoreCase);
                }
                else if (arg.Contains(nameof(WSUser.Id).ToLower()))
                {
                    createPath = createPath.Replace(arg, user.Id);
                }
            }

            if (!FolderBuilder.CreateFolder(createPath))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: Failed to create folder {createPath}");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var userDirectory = new ServerUserDirectory()
            {
                Name = "User Files",
                Path = createPath,
                Default = true,
                UserId = user.Id,
                AccessFlags = DirectoryAccessFlags.CreateFolder | DirectoryAccessFlags.DeleteFile | DirectoryAccessFlags.DeleteFolder
                    | DirectoryAccessFlags.ReadFile | DirectoryAccessFlags.ReadFolder | DirectoryAccessFlags.UploadFile
            };
            _context.ServerUserDirectory.Add(userDirectory);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CreateDefaultFolder)}: Error saving when creating default user directory");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }
    }
}
