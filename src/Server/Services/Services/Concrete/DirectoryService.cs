using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using VueServer.Core.Helper;
using VueServer.Core.Objects;
using VueServer.Domain.Enums;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Context;
using VueServer.Models.Directory;
using VueServer.Models.Request;
using VueServer.Models.Response;
using VueServer.Models.User;
using VueServer.Services.Interface;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Enums;
using static VueServer.Domain.DomainConstants.Authentication;

namespace VueServer.Services.Concrete
{
    public class DirectoryService : IDirectoryService
    {
        private readonly ILogger _logger;
        private readonly IUserService _user;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IWSContext _wSContext;

        public DirectoryService(ILoggerFactory logger, IUserService user, IWebHostEnvironment env, IConfigurationRoot config, IWSContext wSContext)
        {
            _logger = logger?.CreateLogger<DirectoryService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _config = config ?? throw new ArgumentNullException("Configuration null");
            _wSContext = wSContext ?? throw new ArgumentNullException("WS Context null");
        }

        # region -> Public Functions

        public async Task<IResult<IEnumerable<ServerDirectory>>> GetDirectories()
        {
            var dirs = await GetSingleDirectoryList(_user.Id, true);
            return new Result<IEnumerable<ServerDirectory>>(dirs, StatusCode.OK);
        }

        public async Task<IResult<Tuple<string, string, string>>> Download(string filename, string user, bool media = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Filename passed is null or empty");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            var tuple = GetStrippedFilename(filename);
            if (tuple == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error stripping filename. Null Tuple object");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1 == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error stripping filename. Null Tuple.Item1");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (tuple.Item2 == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error stripping filename. Null Tuple.Item2");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1.Count() == 0)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Filename contains no folders. Invalid filename");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            var list = await GetSingleDirectoryList(user);
            var folders = tuple.Item1.ToList();

            var baseDirObj = list.Where(a => a.Name == folders[0]).FirstOrDefault();
            if (baseDirObj == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid folder name: {folders[0]}");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(baseDirObj.Path))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Path of object is null");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (!baseDirObj.AccessFlags.HasFlag(DirectoryAccessFlags.ReadFile))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: No read file access to files in this folder");
                return new Result<Tuple<string, string, string>>(null, StatusCode.UNAUTHORIZED);
            }

            var cleanFilename = tuple.Item2;
            var sourcePath = GetSourcePath(baseDirObj.Path, folders);
            var sourceFile = GetSourceFile(sourcePath, cleanFilename);
            var contentType = MimeTypeHelper.GetMimeType(sourceFile);
            Tuple<string, string> mediaFilename = null;

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
                            _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Zip file already exists. Serving it as is instead of rebuilding the file.");
                        }
                        else
                        {
                            ZipFile.CreateFromDirectory(sourceFile, zipPath);
                            Thread.Sleep(250);
                        }

                    }
                    catch (ArgumentException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: ArgumentException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (PathTooLongException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: PathTooLongException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: DirectoryNotFoundException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (IOException e)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: IOException");
                        _logger.LogError(e.StackTrace);
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: UnauthorizedException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (NotSupportedException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: NotSupportedException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }

                    _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Private zip archive download begun by " + user + " @ " + _user.IP + " - name=" + filename);
                    return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(zipPath, contentType, cleanFilename), StatusCode.OK);
                }
                //else
                //{
                //    if (media)
                //    {
                //        mediaFilename = await ConvertFile(file);
                //        var newName = GetStrippedFilename(mediaFilename.Item1);
                //        cleanFilename = tuple.Item2;
                //        // Convert successful
                //    }
                //}
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Unknown exception accessing file");
                return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
            }

            _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Private download begun by " + user + " @ " + _user.IP + " - name=" + filename);
            //if (!media)
            return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(Path.Combine(sourcePath, cleanFilename), contentType, cleanFilename), StatusCode.OK);
            //else
            //    return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(mediaFilename.Item1, mediaFilename.Item2, cleanFilename), StatusCode.OK);
        }

        public async Task<IResult<IOrderedEnumerable<WebServerFile>>> Load(string directory, string subDir)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Directory is null. Can't get list.");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            var list = await GetSingleDirectoryList(_user.Id);
            var serverDirectory = list.Where(a => a.Name == directory).FirstOrDefault();
            if (serverDirectory == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid folder name: {directory}");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            var basePath = serverDirectory.Path;
            if (string.IsNullOrWhiteSpace(basePath))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Server directory has a null path");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            string fullPath = string.Empty;
            Uri uri = null;

            if (!string.IsNullOrWhiteSpace(subDir))
            {
                if (!serverDirectory.AccessFlags.HasFlag(DirectoryAccessFlags.ReadFolder))
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Server directory has a null path");
                    return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.UNAUTHORIZED);
                }

                fullPath = Path.Combine(basePath, subDir);
                // Security check to ensure the user isn't trying a path escalation attack
                try
                {
                    uri = new Uri(fullPath);
                    if (!uri.AbsolutePath.ToLower().StartsWith(basePath.ToLower()) && !uri.LocalPath.ToLower().StartsWith(basePath.ToLower()))
                    {
                        _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: {_user.Id} @ {_user.IP} is trying to do a path escalation attack!");
                        return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.FORBIDDEN);
                    }
                }
                catch
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: {_user.Id} @ {_user.IP} sent malformed URI path in sub directory.");
                    return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
                }
            }
            else
            {
                fullPath = basePath;
            }

            DirectoryInfo dir = null;
            FileSystemInfo[] files;
            try
            {
                if (uri == null)
                    uri = new Uri(fullPath);
                dir = new DirectoryInfo(uri.LocalPath);
                if (!dir.Exists)
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Directory ({basePath}) does not exist. User: {_user.Id} @ IP: {_user.IP}");
                    return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
                }

                files = dir.GetFileSystemInfos();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error getting directory or file system info at: {basePath}");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            var fileList = new List<WebServerFile>();
            foreach (var file in files)
            {
                fileList.Add(new WebServerFile(file));
            }

            return new Result<IOrderedEnumerable<WebServerFile>>(fileList.OrderByDescending(x => x.IsFolder).ThenBy(x => x.Title), StatusCode.OK);
        }

        public async Task<IResult<WebServerFile>> CreateFolder(string directory, string subDir, string newFolder)
        {
            if (directory == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Directory is null. Can't create folder if we don't know where it's located.");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (newFolder == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: New Folder is null. Can't create a folder without a name.");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            var list = await GetSingleDirectoryList(_user.Id);
            var serverDirectory = list.Where(a => a.Name == directory).FirstOrDefault();
            if (serverDirectory == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid folder name: {directory}");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (!serverDirectory.AccessFlags.HasFlag(DirectoryAccessFlags.CreateFolder))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User doesn't have folder creation access for this folder: {directory}\\{subDir}");
                return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
            }

            string path = string.Empty;
            DirectoryInfo info = null;

            if (string.IsNullOrWhiteSpace(subDir))
            {
                path = Path.Combine(serverDirectory.Path, newFolder);
            }
            else
            {
                path = Path.Combine(serverDirectory.Path, subDir, newFolder);
                if (!path.StartsWith(serverDirectory.Path))
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Looks like a path escalation attack from user ({_user.Id}) for path: {path}");
                    return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
                }
            }

            try
            {
                info = Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating directory ({path}) for user ({_user.Id})");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            return new Result<WebServerFile>(new WebServerFile(info), StatusCode.OK);
        }

        public async Task<IResult<WebServerFile>> Upload(UploadDirectoryFileRequest model)
        {
            var dirList = await GetSingleDirectoryList(_user.Id);
            var dir = dirList.Where(x => x.Name == model.Directory).FirstOrDefault();
            if (dir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid folder name provided. File upload attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (!dir.AccessFlags.HasFlag(DirectoryAccessFlags.UploadFile))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: {_user.Id} @ {_user.IP} is attempting to upload to a folder that doesn't allow file uploading regardless of their permissions");
                return new Result<WebServerFile>(null, StatusCode.UNAUTHORIZED);
            }

            string baseSaveDir = dir.Path;
            string saveDir = baseSaveDir;

            if (!string.IsNullOrEmpty(model.SubDirectory))
            {
                saveDir += model.SubDirectory;
            }

            _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Upload started from " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);

            if (!Directory.Exists(saveDir))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: {_user.Id} @ {_user.IP} is attempting to create a folder while uploading a file");
                return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
            }

            try
            {
                var uri = new Uri(saveDir);
                if (!uri.LocalPath.StartsWith(baseSaveDir))
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: {_user.Id} @ {_user.IP} is trying to do a path escalation attack!");
                    return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
                }
            }
            catch
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: {_user.Id} @ {_user.IP} sent malformed URI path.");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            try
            {
                using (FileStream fs = File.Create(Path.Combine(saveDir, model.File.FileName)))
                {
                    await model.File.CopyToAsync(fs);
                    fs.Flush();
                    _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Upload SUCCESS from " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Failed to write file dir\n" + e.StackTrace);
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Upload FAILED from " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            try
            {
                var file = new FileInfo(Path.Combine(saveDir, model.File.FileName));
                var webServerFile = new WebServerFile(file);
                return new Result<WebServerFile>(webServerFile, StatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Failed to write file dir\n" + e.StackTrace);
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Failed to validate file exists after successful upload. " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IResult<bool>> Delete(FileModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new Result<bool>(false, StatusCode.BAD_REQUEST);

            var dirList = await GetSingleDirectoryList(_user.Id);
            var dir = dirList.Where(x => x.Name == model.Directory).FirstOrDefault();
            if (dir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid folder name provided. Protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            if (!dir.AccessFlags.HasFlag(DirectoryAccessFlags.DeleteFile))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Unable to delete folder. User: " + _user.Id + " @ " + _user.IP + " doesn't have permission to delete this folder.");
                return new Result<bool>(false, StatusCode.UNAUTHORIZED);
            }

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(Path.Combine(dir.Path, model.SubDirectory, model.Name));
            }
            catch
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid file provided. Protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<bool>(false, StatusCode.UNAUTHORIZED);
            }

            if (!fi.Directory.FullName.StartsWith(dir.Path))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Path escalation attack attempted by {_user.Id} @ {_user.IP} - Filename={fi.Directory.FullName.ToString()}{Path.DirectorySeparatorChar}{model.Name}!");
                return new Result<bool>(false, StatusCode.FORBIDDEN);
            }

            if (fi.Exists)
            {
                try
                {
                    fi.Delete();
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Unauthorized protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
                catch (SecurityException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Security exception in protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
                catch (IOException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: IO exception in protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<bool>(false, StatusCode.BAD_REQUEST);
                }
            }


            return new Result<bool>(true, StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        private async Task<string> GetMaxRole(WSUser user)
        {
            var roles = await _user.GetUserRolesAsync(user);

            if (roles.Contains(ADMINISTRATOR_STRING))
                return ADMINISTRATOR_STRING;
            else if (roles.Contains(ELEVATED_STRING))
                return ELEVATED_STRING;
            else if (roles.Contains(USER_STRING))
                return USER_STRING;
            else
                return null;
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

                if (cleaned.Length == index + 1)
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Filename ends with a slash. Invalid filename.");
                    return null;
                }
                folders.Add(cleaned.Substring(0, index));
                cleaned = cleaned.Substring(index + 1);
            }

            return new Tuple<IEnumerable<string>, string>(folders, cleaned);
        }

        private string GetSourcePath(string dir, IList<string> folders)
        {
            // TODO: Add logger stuff for these null checks
            if (folders == null || folders.Count == 0)
                return null;

            if (dir == null)
                return null;

            string fn = string.Copy(dir);
            if (!dir.Contains(Path.DirectorySeparatorChar))
                fn += Path.DirectorySeparatorChar;

            for (int i = 0; i < folders.Count; i++)
            {
                // Ignore first folder as it is not necessarily a valid value. That is what the dir variable is
                if (i == 0)
                    continue;

                if (fn[fn.Length - 1] != Path.DirectorySeparatorChar)
                    fn += Path.DirectorySeparatorChar;

                fn += folders[i];
            }

            return fn;
        }

        private string GetSourceFile(string dir, string filename)
        {
            // TODO: Add logger stuff for these null checks
            if (dir == null)
                return null;

            if (filename == null)
                return null;

            return Path.Combine(dir, filename);
        }

        private async Task<IEnumerable<ServerUserDirectory>> GetServerUserDirectoryList(WSUser user)
        {
            if (user == null)
                return new List<ServerUserDirectory>();

            return await _wSContext.ServerUserDirectory.Where(x => x.UserId == user.Id).ToListAsync();
        }

        private async Task<IEnumerable<ServerGroupDirectory>> GetServerGroupDirectoryList(string role)
        {
            if (role == null)
                return new List<ServerGroupDirectory>();

            return await _wSContext.ServerGroupDirectory.Where(x => x.Role == role).ToListAsync();
        }

        private async Task<IList<ServerDirectory>> GetSingleDirectoryList(string user, bool stripPath = false)
        {
            var wsUser = await _user.GetUserByIdAsync(user);

            var groupDirs = await GetServerGroupDirectoryList(await GetMaxRole(wsUser));
            var userDirs = await GetServerUserDirectoryList(wsUser);

            var allDirs = new List<ServerDirectory>();
            if (groupDirs != null)
            {
                foreach (var dir in groupDirs)
                {
                    allDirs.Add(new ServerDirectory()
                    {
                        Name = dir.Name,
                        Default = false,
                        Path = stripPath ? null : dir.Path,
                        AccessFlags = dir.AccessFlags
                    });
                }
            }

            if (userDirs != null)
            {
                foreach (var dir in userDirs)
                {
                    allDirs.Add(new ServerDirectory()
                    {
                        Name = dir.Name,
                        Default = dir.Default,
                        Path = stripPath ? null : dir.Path,
                        AccessFlags = dir.AccessFlags
                    });
                }
            }

            return allDirs;
        }

        /// <summary>
        /// Work in progress for live transcoding
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<Tuple<string, string>> ConvertFile(FileInfo file)
        {
            var format = ".mp4";
            //Save file to the same location with changed extension
            //string outputFileName = Path.ChangeExtension(file.FullName, ".mp4");
            string outputFileName = Path.Combine(_env.WebRootPath, "tmp", Guid.NewGuid().ToString() + format);

            var mediaInfo = await MediaInfo.Get(file);
            var videoStream = mediaInfo.VideoStreams.First();
            var audioStream = mediaInfo.AudioStreams.First();

            //Change some parameters of video stream
            videoStream
                //Set size to 480p
                .SetSize(VideoSize.Hd480)
                //Set codec which will be used to encode file. If not set it's set automatically according to output file extension
                .SetCodec(VideoCodec.H264);

            //Create new conversion object
            var conversion = Conversion.New()
                //Add video stream to output file
                .AddStream(videoStream)
                //Add audio stream to output file
                .AddStream(audioStream)
                //Set output file path
                .SetOutput(outputFileName)
                //SetOverwriteOutput to overwrite files. It's useful when we already run application before
                .SetOverwriteOutput(true)
                //Enable multithreading
                .UseMultiThread(true)
                //Set conversion preset. You have to chose between file size and quality of video and duration of conversion
                .SetPreset(ConversionPreset.UltraFast);
            //Add log to OnProgress
            conversion.OnProgress += async (sender, args) =>
            {
                //Show all output from FFmpeg to console
                await Console.Out.WriteLineAsync($"[{args.Duration}/{args.TotalLength}][{args.Percent}%] {file.Name}");
            };
            //Start conversion
            await conversion.Start();

            await Console.Out.WriteLineAsync($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Finished converion file [{file.Name}]");
            return new Tuple<string, string>(outputFileName, format);
        }

        //private async Task<Tuple<string, string>> ConvertFile (FileInfo file)
        //{
        //    var format = ".mp4";
        //    //Save file to the same location with changed extension
        //    //string outputFileName = Path.ChangeExtension(file.FullName, ".mp4");
        //    string outputFileName = Path.Combine(_env.WebRootPath, "tmp", Guid.NewGuid().ToString() + format);

        //    //SetOverwriteOutput to overwrite files. It's useful when we already run application before
        //    var conversion = Conversion.ToMp4(file.FullName, outputFileName).SetOverwriteOutput(true);
        //    //var conversion = Conversion.ToWebM(file.FullName, outputFileName);
        //    //Add log to OnProgress
        //    conversion.OnProgress += async (sender, args) =>
        //    {
        //        //Show all output from FFmpeg to console
        //        await Console.Out.WriteLineAsync($"[{args.Duration}/{args.TotalLength}][{args.Percent}%] {file.Name}");
        //    };
        //    //Start conversion
        //    await conversion.Start();

        //    await Console.Out.WriteLineAsync($"Finished converion file [{file.Name}]");
        //    return new Tuple<string, string>(outputFileName, format);
        //}

        #endregion
    }
}
