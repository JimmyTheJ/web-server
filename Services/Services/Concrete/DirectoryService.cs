using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using VueServer.Common.Helper;
using VueServer.Common.Interface;
using VueServer.Common.Concrete;
using VueServer.Services.Interface;
using static VueServer.Common.Constants;
using VueServer.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VueServer.Models.Directory;
using System.Collections;

namespace VueServer.Services.Concrete
{
    public class DirectoryService : IDirectoryService
    {
        private ILogger _logger { get; set; }

        private IUserService _user { get; set; }

        private IHostingEnvironment _env { get; set; }

        private IConfiguration _config { get; set; }

        public DirectoryService(ILoggerFactory logger, IUserService user, IHostingEnvironment env, IConfiguration config)
        {
            _logger = logger.CreateLogger<DirectoryService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _config = config ?? throw new ArgumentNullException("Configuration null");
        }

        # region -> Public Functions

        public IResult<IEnumerable<string>> GetDirectories (int level = NO_LEVEL)
        {
            var maxLevel = GetMaxLevel();
            if (level > maxLevel || maxLevel == 0)
            {
                _logger.LogWarning("Directory.GetDirectories: Permission escalation attack attempted. Setting level to the lowest setting.");
                level = GENERAL;
            }

            var dirs = GetSingleDirectoryList(level);
            return new Result<IEnumerable<string>>(dirs.Select(x => x.Name), Common.Enums.StatusCode.OK);
        }

        public IResult<Tuple<string, string, string>> Download (int level, string filename)
        { 
            if (string.IsNullOrWhiteSpace(filename))
            {
                _logger.LogWarning("Directory.Download: Filename passed is null or empty");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            var maxLevel = GetMaxLevel();
            if (level > maxLevel || maxLevel == 0)
            {
                _logger.LogWarning("Directory.Download: Invalid access level");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.UNAUTHORIZED);
            }

            var tuple = GetStrippedFilename(filename);
            if (tuple == null)
            {
                _logger.LogWarning("Directory.Download: Error stripping filename. Null Tuple object");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1 == null)
            {
                _logger.LogWarning("Directory.Download: Error stripping filename. Null Tuple.Item1");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            if (tuple.Item2 == null)
            {
                _logger.LogWarning("Directory.Download: Error stripping filename. Null Tuple.Item2");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1.Count() == 0)
            {
                _logger.LogWarning("Directory.Download: Filename contains no folders. Invalid filename");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            var list = GetSingleDirectoryList(level).ToList();
            var folders = tuple.Item1.ToList();

            var baseDir = list.Where(a => a.Name == folders[0]).Select(a => a.Path).FirstOrDefault();
            if (baseDir == null)
            {
                _logger.LogWarning($"Directory.Download: Invalid folder name: {folders[0]}");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            var cleanFilename = tuple.Item2;
            var sourcePath = GetSourcePath(baseDir, folders);
            var sourceFile = GetSourceFile(sourcePath, cleanFilename);
            var contentType = MimeTypeHelper.GetMimeType(sourceFile);

            try
            {
                // Check if the file exists on the server and if it's a directory zip it
                var file = new FileInfo(sourceFile);
                if (file.Attributes.HasFlag(FileAttributes.Directory))
                {
                    contentType = "application/zip";
                    cleanFilename += ".zip";
                    string zipPath = Path.Combine(_env.WebRootPath, "tmp", cleanFilename);
                    
                    try
                    {
                        if (File.Exists(zipPath))
                        {
                            _logger.LogInformation("Directory.Download: Zip file already exists. Serving it as is instead of rebuilding the file.");
                        }
                        else
                        {
                            ZipFile.CreateFromDirectory(sourceFile, zipPath);
                            Thread.Sleep(250);
                        }

                    }
                    catch (ArgumentException)
                    {
                        _logger.LogError("DownloadProtectedFile: ArgumentException");
                        return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (PathTooLongException)
                    {
                        _logger.LogError("DownloadProtectedFile: PathTooLongException");
                        return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _logger.LogError("DownloadProtectedFile: DirectoryNotFoundException");
                        return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (IOException e)
                    {
                        _logger.LogError("DownloadProtectedFile: IOException");
                        _logger.LogError(e.StackTrace);
                        return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.LogError("DownloadProtectedFile: UnauthorizedException");
                        return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (NotSupportedException)
                    {
                        _logger.LogError("DownloadProtectedFile: NotSupportedException");
                        return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.SERVER_ERROR);
                    }

                    _logger.LogInformation("Private zip archive download begun by " + _user.Name + " @ " + _user.IP + " - name=" + filename);
                    return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(zipPath, contentType, cleanFilename), Common.Enums.StatusCode.OK);
                }
            }
            catch (Exception)
            {
                _logger.LogError("DownloadProtectedFile: Unknown exception accessing file");
                return new Result<Tuple<string, string, string>>(null, Common.Enums.StatusCode.SERVER_ERROR);
            }

            _logger.LogInformation("Private download begun by " + _user.Name + " @ " + _user.IP + " - name=" + filename);
            return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(Path.Combine(sourcePath, cleanFilename), contentType, cleanFilename), Common.Enums.StatusCode.OK);
        }

        public IResult<IOrderedEnumerable<WebServerFile>> Load (string directory, string subDir, int level)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                _logger.LogWarning("Directory.Load: Directory is null. Can't get list.");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            DirectoryInfo dir = null;
            var fileList = new List<WebServerFile>();
            int maxLevel = GetMaxLevel();
            if (level > maxLevel || maxLevel == 0)
            {
                _logger.LogWarning($"Directory.Load: Permission escalation attempt, sending a level too high for user");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, Common.Enums.StatusCode.FORBIDDEN);
            }

            var list = GetSingleDirectoryList(level).ToList();
            var path = list.Where(a => a.Name == directory).Select(a => a.Path).FirstOrDefault();
            if (path == null)
            {
                _logger.LogWarning($"Directory.Load: Invalid folder name: {directory}");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            if (subDir != null)
                path = Path.Combine(path, subDir);  // TODO: Check if this allows user to go backwards in path (SECURITY ISSUE)

            try
            {
                dir = new DirectoryInfo(path);
            }
            catch (Exception)
            {
                return new Result<IOrderedEnumerable<WebServerFile>>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            var files = dir.GetFileSystemInfos();
            foreach (var file in files)
            {
                fileList.Add(new WebServerFile(file));
            }

            return new Result<IOrderedEnumerable<WebServerFile>>(fileList.OrderByDescending(x => x.IsFolder).ThenBy(x => x.Title), Common.Enums.StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        private int GetMaxLevel ()
        {
            if (_user.Context.User.IsInRole("Administrator") )
                return ADMIN;
            else if (_user.Context.User.IsInRole("Elevated") )
                return ELEVATED;
            else if (_user.Context.User.IsInRole("User") )
                return GENERAL;
            else
                return 0;
        }

        private string GetMaxLevelString ()
        {
            if (_user.Context.User.IsInRole("Administrator") )
                return ADMIN_STRING;
            else if (_user.Context.User.IsInRole("Elevated") )
                return ELEVATED_STRING;
            else if (_user.Context.User.IsInRole("User") )
                return GENERAL_STRING;
            else
                return INVALID_STRING;
        }

        private Tuple<IEnumerable<string>, string> GetStrippedFilename(string fn)
        {
            var folders = new List<string>();
            var cleaned = string.Copy(fn);

            while (cleaned.Contains('/') || cleaned.Contains('\\'))
            {
                var index = new int[] { cleaned.IndexOf('/'), cleaned.IndexOf('\\') }.Min();
                if (index == -1)
                    index = new int[] { cleaned.IndexOf('/'), cleaned.IndexOf('\\') }.Max();

                if (cleaned.Length == index+1)
                {
                    _logger.LogWarning("Directory.GetStrippedFilename: Filename ends with a slash. Invalid filename.");
                    return null;
                }
                folders.Add(cleaned.Substring(0, index));
                cleaned = cleaned.Substring(index + 1);
            }

            return new Tuple<IEnumerable<string>, string>(folders, cleaned);
        }

        private string GetSourcePath (string dir, IList<string> folders)
        {
            // TODO: Add logger stuff for these null checks
            if (folders == null || folders.Count == 0)
                return null;

            if (dir == null)
                return null;

            string fn = string.Copy(dir);
            if (!dir.Contains('\\'))
                fn += "\\";

            for (int i = 0; i < folders.Count; i++)
            {
                // Ignore first folder as it is not necessarily a valid value. That is what the dir variable is
                if (i == 0)
                    continue;

                if (fn[fn.Length-1] != '\\')
                    fn += "\\";
                
                fn += folders[i];
            }

            return fn;
        }

        private string GetSourceFile (string dir, string filename)
        {
            // TODO: Add logger stuff for these null checks
            if (dir == null)
                return null;

            if (filename == null)
                return null;

            return Path.Combine(dir, filename);
        }

        private ServerDirectoryLists GetServerDirectoryLists()
        {
            ServerDirectoryLists dirs = new ServerDirectoryLists
            {
                Admin = GetServerDirectoryList("Directories:Admin"),
                Elevated = GetServerDirectoryList("Directories:Elevated"),
                General = GetServerDirectoryList("Directories:General")
            };

            return dirs;
        }

        private IList<ServerDirectory> GetServerDirectoryList (string configPath)
        {
            IList<ServerDirectory> list = new List<ServerDirectory>();

            try
            {
                var section = _config.GetSection(configPath);
                if (section == null)
                {
                    _logger.LogInformation("[Directory Service] GetServerDirectoryList: Section doesn't exist");
                    return list;
                }

                foreach (IConfigurationSection s in section.GetChildren())
                {
                    list.Add(new ServerDirectory(s.GetValue<string>("Name"), s.GetValue<string>("Path")));
                }
            }
            catch (Exception)
            {
                _logger.LogWarning("Error deserializing directories from appsettings json");
            }

            return list;
        }

        private IList<ServerDirectory> GetSingleDirectoryList (int level = NO_LEVEL)
        {
            var dirs = GetServerDirectoryLists();

            if (level == NO_LEVEL)
                level = GetMaxLevel();

            switch (level)
            {
                case ADMIN: 
                    return dirs.Admin;
                case ELEVATED: 
                    return dirs.Elevated;
                case GENERAL: 
                    return dirs.General;
                default:
                    return new List<ServerDirectory>();
            }
        }

        #endregion
    }
}
