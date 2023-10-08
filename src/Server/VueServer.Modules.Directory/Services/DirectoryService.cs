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
using VueServer.Modules.Core.Models.User;
using VueServer.Modules.Core.Services.User;
using VueServer.Modules.Directory.Context;
using VueServer.Modules.Directory.Models;
using VueServer.Modules.Directory.Models.Request;
using VueServer.Modules.Directory.Models.Response;
using Xabe.FFmpeg;
using static VueServer.Domain.DomainConstants.Authentication;

namespace VueServer.Modules.Directory.Services
{
    public class DirectoryService : IDirectoryService
    {
        private const string FileString = "File";
        private const string FolderString = "Folder";
        private const string CopyString = " - Copy";

        private readonly ILogger _logger;
        private readonly IUserService _user;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IDirectoryContext _context;

        public DirectoryService(ILoggerFactory logger, IUserService user, IWebHostEnvironment env, IConfigurationRoot config, IDirectoryContext context)
        {
            _logger = logger?.CreateLogger<DirectoryService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _config = config ?? throw new ArgumentNullException("Configuration null");
            _context = context ?? throw new ArgumentNullException("Directory Context null");
        }

        # region -> Public Functions

        public async Task<IServerResult<IEnumerable<ServerDirectory>>> GetDirectories()
        {
            var dirs = await GetSingleDirectoryList(_user.Id, true);
            return new Result<IEnumerable<ServerDirectory>>(dirs, StatusCode.OK);
        }

        public async Task<IServerResult<Tuple<string, string, string>>> Download(string filename, string user, bool media = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Download)}: Filename passed is null or empty");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            var tuple = GetStrippedFilename(filename);
            if (tuple == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Download)}: Error stripping filename. Null Tuple object");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1 == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Download)}: Error stripping filename. Null Tuple.Item1");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (tuple.Item2 == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Download)}: Error stripping filename. Null Tuple.Item2");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1.Count() == 0)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Download)}: Filename contains no folders. Invalid filename");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            var list = await GetSingleDirectoryList(user);
            var folders = tuple.Item1.ToList();

            var baseDirObj = list.Where(a => a.Name == folders[0]).FirstOrDefault();
            if (baseDirObj == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Download)}: Invalid folder name: {folders[0]}");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (string.IsNullOrWhiteSpace(baseDirObj.Path))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Download)}: Path of object is null");
                return new Result<Tuple<string, string, string>>(null, StatusCode.BAD_REQUEST);
            }

            if (!baseDirObj.AccessFlags.HasFlag(DirectoryAccessFlags.ReadFile))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Download)}: No read file access to files in this folder");
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
                            _logger.LogInformation($"[{this.GetType().Name}] {nameof(Download)}: Zip file already exists. Serving it as is instead of rebuilding the file.");
                        }
                        else
                        {
                            ZipFile.CreateFromDirectory(sourceFile, zipPath);
                            Thread.Sleep(250);
                        }

                    }
                    catch (ArgumentException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {nameof(Download)}: ArgumentException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (PathTooLongException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {nameof(Download)}: PathTooLongException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {nameof(Download)}: DirectoryNotFoundException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (IOException e)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {nameof(Download)}: IOException");
                        _logger.LogError(e.StackTrace);
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {nameof(Download)}: UnauthorizedException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }
                    catch (NotSupportedException)
                    {
                        _logger.LogError($"[{this.GetType().Name}] {nameof(Download)}: NotSupportedException");
                        return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
                    }

                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(Download)}: Private zip archive download begun by " + user + " @ " + _user.IP + " - name=" + filename);
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
                _logger.LogError($"[{this.GetType().Name}] {nameof(Download)}: Unknown exception accessing file");
                return new Result<Tuple<string, string, string>>(null, StatusCode.SERVER_ERROR);
            }

            _logger.LogInformation($"[{this.GetType().Name}] {nameof(Download)}: Private download begun by " + user + " @ " + _user.IP + " - name=" + filename);
            //if (!media)
            return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(Path.Combine(sourcePath, cleanFilename), contentType, cleanFilename), StatusCode.OK);
            //else
            //    return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(mediaFilename.Item1, mediaFilename.Item2, cleanFilename), StatusCode.OK);
        }

        public async Task<IServerResult<IOrderedEnumerable<WebServerFile>>> Load(string directory, string subDir)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Load)}: Directory is null. Can't get list.");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            var list = await GetSingleDirectoryList(_user.Id);
            var serverDirectory = list.Where(a => a.Name == directory).FirstOrDefault();
            if (serverDirectory == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Load)}: Invalid folder name: {directory}");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            var basePath = serverDirectory.Path;
            if (string.IsNullOrWhiteSpace(basePath))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Load)}: Server directory has a null path");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            string fullPath = string.Empty;
            Uri uri = null;

            if (!string.IsNullOrWhiteSpace(subDir))
            {
                if (!serverDirectory.AccessFlags.HasFlag(DirectoryAccessFlags.ReadFolder))
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(Load)}: Server directory has a null path");
                    return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.UNAUTHORIZED);
                }

                fullPath = Path.Combine(basePath, subDir);
                // Security check to ensure the user isn't trying a path escalation attack
                try
                {
                    uri = new Uri(fullPath);
                    if (!uri.AbsolutePath.ToLower().StartsWith(basePath.ToLower()) && !uri.LocalPath.ToLower().StartsWith(basePath.ToLower()))
                    {
                        _logger.LogWarning($"[{this.GetType().Name}] {nameof(Load)}: {_user.Id} @ {_user.IP} is trying to do a path escalation attack!");
                        return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.FORBIDDEN);
                    }
                }
                catch
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(Load)}: {_user.Id} @ {_user.IP} sent malformed URI path in sub directory.");
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
                {
                    uri = new Uri(fullPath);
                }

                dir = new DirectoryInfo(uri.LocalPath);
                if (!dir.Exists)
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {nameof(Load)}: Directory ({basePath}) does not exist. User: {_user.Id} @ IP: {_user.IP}");
                    return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
                }

                files = dir.GetFileSystemInfos();
            }
            catch (Exception)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Load)}: Error getting directory or file system info at: {basePath}");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, StatusCode.BAD_REQUEST);
            }

            var fileList = new List<WebServerFile>();
            foreach (var file in files)
            {
                fileList.Add(new WebServerFile(file));
            }

            return new Result<IOrderedEnumerable<WebServerFile>>(fileList.OrderByDescending(x => x.IsFolder).ThenBy(x => x.Title), StatusCode.OK);
        }

        public async Task<IServerResult<WebServerFile>> CreateFolder(string directory, string subDir, string newFolder)
        {
            if (directory == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CreateFolder)}: Directory is null. Can't create folder if we don't know where it's located.");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (newFolder == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CreateFolder)}: New Folder is null. Can't create a folder without a name.");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            var list = await GetSingleDirectoryList(_user.Id);
            var serverDirectory = list.Where(a => a.Name == directory).FirstOrDefault();
            if (serverDirectory == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CreateFolder)}: Invalid folder name: {directory}");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (!serverDirectory.AccessFlags.HasFlag(DirectoryAccessFlags.CreateFolder))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CreateFolder)}: User doesn't have folder creation access for this folder: {directory}\\{subDir}");
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
                    _logger.LogWarning($"[{this.GetType().Name}] {nameof(CreateFolder)}: Looks like a path escalation attack from user ({_user.Id}) for path: {path}");
                    return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
                }
            }

            try
            {
                info = System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating directory ({path}) for user ({_user.Id})");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            return new Result<WebServerFile>(new WebServerFile(info), StatusCode.OK);
        }

        public async Task<IServerResult<WebServerFile>> RenameFile(MoveFileRequest model)
        {
            var dirList = await GetSingleDirectoryList(_user.Id);
            var dir = dirList.Where(x => x.Name == model.Directory).FirstOrDefault();

            if (dir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFile)}: Invalid folder name provided. File rename attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (!dir.AccessFlags.HasFlag(DirectoryAccessFlags.MoveFile))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFile)}: {_user.Id} @ {_user.IP} is attempting to rename a file in a folder that doesn't allow file moving");
                return new Result<WebServerFile>(null, StatusCode.UNAUTHORIZED);
            }

            if (model.Name == model.NewName)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFile)}: New filename and old filename are the same. Can't rename a file to it's original name");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            string basePath = string.IsNullOrWhiteSpace(model.SubDirectory) ? dir.Path : Path.Combine(dir.Path, model.SubDirectory);
            string oldFullPath = Path.Combine(basePath, model.Name);

            // Ensure this the original file is valid
            FileInfo oldFileInfo = null;
            try
            {
                oldFileInfo = new FileInfo(oldFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(RenameFile)}: Can't get FileInfo on the old file ({oldFullPath}). Something must have gone wrong. Possible attack vector");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            string newFullPath = Path.Combine(basePath, model.NewName);

            // Use the platform specific set of invalid characters to ensure the new file name will be valid
            var invalidCharacters = Path.GetInvalidPathChars();
            if (invalidCharacters.Any(x => newFullPath.Contains(x)))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFile)}: New filename contains invalid characters");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (IsPathEscalation(newFullPath) || !oldFullPath.StartsWith(dir.Path) || !newFullPath.StartsWith(dir.Path))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(RenameFile)}: Looks like a path escalation attack from user ({_user.Id}) for path: {newFullPath}");
                return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
            }

            if (File.Exists(newFullPath))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFile)}: New filename selected already exists. Can't rename a file to an existing file name of {newFullPath}");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            try
            {
                File.Move(oldFullPath, newFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(RenameFile)}: Error copying file {newFullPath}");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            FileInfo fileInfo = null;
            try
            {
                fileInfo = new FileInfo(newFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(RenameFile)}: Can't get FileInfo on the newly moved file. Something must have gone wrong.");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            return new Result<WebServerFile>(new WebServerFile(fileInfo), StatusCode.OK);
        }

        public async Task<IServerResult<WebServerFile>> RenameFolder(MoveFileRequest model)
        {
            var dirList = await GetSingleDirectoryList(_user.Id);
            var dir = dirList.Where(x => x.Name == model.Directory).FirstOrDefault();

            if (dir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFolder)}: Invalid folder name provided. Folder rename attempt by " + _user.Id + " @ " + _user.IP + " - Foldername=" + model.Name);
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (!dir.AccessFlags.HasFlag(DirectoryAccessFlags.MoveFolder))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFolder)}: {_user.Id} @ {_user.IP} is attempting to rename a folder that doesn't allow folder moving");
                return new Result<WebServerFile>(null, StatusCode.UNAUTHORIZED);
            }

            if (model.Name == model.NewName)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFolder)}: New folder and old folder are the same. Can't rename a folder to it's original name");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            string basePath = string.IsNullOrWhiteSpace(model.SubDirectory) ? dir.Path : Path.Combine(dir.Path, model.SubDirectory);
            string oldFullPath = Path.Combine(basePath, model.Name);

            // Ensure this the original file is valid
            DirectoryInfo oldFileInfo = null;
            try
            {
                oldFileInfo = new DirectoryInfo(oldFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(RenameFolder)}: Can't get FileInfo / DirectoryInfo on the old file / folder ({oldFullPath}). Something must have gone wrong. Possible attack vector");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            string newFullPath = Path.Combine(basePath, model.NewName);

            // Use the platform specific set of invalid characters to ensure the new file name will be valid
            var invalidCharacters = Path.GetInvalidPathChars();
            if (invalidCharacters.Any(x => newFullPath.Contains(x)))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFolder)}: New folder contains invalid characters");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (IsPathEscalation(newFullPath) || !oldFullPath.StartsWith(dir.Path) || !newFullPath.StartsWith(dir.Path))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(RenameFolder)}: Looks like a path escalation attack from user ({_user.Id}) for path: {newFullPath}");
                return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
            }

            if (System.IO.Directory.Exists(newFullPath))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(RenameFolder)}: New folder selected already exists. Can't rename a folder to an existing folder name of {newFullPath}");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            try
            {
                System.IO.Directory.Move(oldFullPath, newFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(RenameFolder)}: Error copying file {newFullPath}");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            DirectoryInfo newDirectoryInfo = null;
            try
            {
                newDirectoryInfo = new DirectoryInfo(newFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(RenameFolder)}: Can't get FileInfo on the newly moved file. Something must have gone wrong.");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            return new Result<WebServerFile>(new WebServerFile(newDirectoryInfo), StatusCode.OK);
        }

        public async Task<IServerResult<WebServerFile>> Upload(UploadDirectoryFileRequest model)
        {
            if (model == null || model.File == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: File upload attempt by " + _user.Id + " @ " + _user.IP + "failed because the model passed to the controller was null.");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            var dirList = await GetSingleDirectoryList(_user.Id);
            var dir = dirList.Where(x => x.Name == model.Directory).FirstOrDefault();
            if (dir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: Invalid folder name provided. File upload attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            if (!dir.AccessFlags.HasFlag(DirectoryAccessFlags.UploadFile))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: {_user.Id} @ {_user.IP} is attempting to upload to a folder that doesn't allow file uploading regardless of their permissions");
                return new Result<WebServerFile>(null, StatusCode.UNAUTHORIZED);
            }

            string baseSaveDir = dir.Path;
            string saveDir = baseSaveDir;

            if (!string.IsNullOrEmpty(model.SubDirectory))
            {
                saveDir += model.SubDirectory;
            }

            _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: Upload started from " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);

            if (!System.IO.Directory.Exists(saveDir))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: {_user.Id} @ {_user.IP} is attempting to create a folder while uploading a file");
                return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
            }

            try
            {
                var uri = new Uri(saveDir);
                if (!uri.LocalPath.StartsWith(baseSaveDir))
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {nameof(Upload)}: {_user.Id} @ {_user.IP} is trying to do a path escalation attack!");
                    return new Result<WebServerFile>(null, StatusCode.FORBIDDEN);
                }
            }
            catch
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: {_user.Id} @ {_user.IP} sent malformed URI path.");
                return new Result<WebServerFile>(null, StatusCode.BAD_REQUEST);
            }

            try
            {
                using (FileStream fs = File.Create(Path.Combine(saveDir, model.File.FileName)))
                {
                    await model.File.CopyToAsync(fs);
                    fs.Flush();
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: Upload SUCCESS from " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{this.GetType().Name}] {nameof(Upload)}: Failed to write file dir\n" + e.StackTrace);
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: Upload FAILED from " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);
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
                Console.WriteLine($"[{this.GetType().Name}] {nameof(Upload)}: Failed to write file dir\n" + e.StackTrace);
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Upload)}: Failed to validate file exists after successful upload. " + _user.Id + " @ " + _user.IP + " - Filename=" + model.File.FileName);
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }
        }

        public async Task<IServerResult<WebServerFile>> CopyFile(FileModel source, FileModel destination)
        {
            var result = await CopyOrMoveValidation(source, destination, true, DirectoryAccessFlags.MoveFile, FileString);
            if (result.Code != StatusCode.OK)
            {
                return new Result<WebServerFile>(null, result.Code);
            }

            // Ensure the original file is valid
            FileInfo oldFileInfo;
            try
            {
                oldFileInfo = new FileInfo(result.SourceFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(CopyFile)}: Can't get FileInfo on the source file ({result.SourceFullPath}). Something must have gone wrong. Possible attack vector");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            // Handle the case where the destination has the same named file in it. We want to still be able to copy here so we append - Copy, and count upwards with ([num]) at the end if additional copies exist
            var newCopyString = HandleSameNameCopy(result, oldFileInfo);
            if (newCopyString != null)
            {
                result.DestinationFullPath = Path.Combine(result.DestinationBasePath, newCopyString);
            }

            try
            {
                File.Copy(result.SourceFullPath, result.DestinationFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(CopyFile)}: Error copying file {result.SourceFullPath} to {result.DestinationFullPath}");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            FileInfo newFileInfo;
            try
            {
                newFileInfo = new FileInfo(result.DestinationFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(CopyFile)}: Can't get FileInfo on the newly copied file. Something must have gone wrong.");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            return new Result<WebServerFile>(new WebServerFile(newFileInfo), StatusCode.OK);
        }

        public async Task<IServerResult<WebServerFile>> CopyFolder(FileModel source, FileModel destination)
        {
            var result = await CopyOrMoveValidation(source, destination, true, DirectoryAccessFlags.MoveFolder, FolderString);
            if (result.Code != StatusCode.OK)
            {
                return new Result<WebServerFile>(null, result.Code);
            }

            // Ensure the original folder is valid
            FileInfo oldFileInfo;
            try
            {
                var oldFolderInfo = new DirectoryInfo(result.SourceFullPath);
                oldFileInfo = new FileInfo(result.SourceFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(CopyFolder)}: Can't get DirectoryInfo on the source folder ({result.SourceFullPath}). Something must have gone wrong. Possible attack vector");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            // Handle the case where the destination has the same named file in it. We want to still be able to copy here so we append - Copy, and count upwards with ([num]) at the end if additional copies exist
            var newCopyString = HandleSameNameCopy(result, oldFileInfo);
            if (newCopyString != null)
            {
                result.DestinationFullPath = Path.Combine(result.DestinationBasePath, newCopyString);
            }

            // Recursively copy folder and subfolders
            try
            {
                CopyDirectory(result.SourceFullPath, result.DestinationFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(CopyFolder)}: Error copying folder {result.SourceFullPath} to {result.DestinationFullPath}");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            DirectoryInfo newDirectoryInfo;
            try
            {
                newDirectoryInfo = new DirectoryInfo(result.DestinationFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(CopyFolder)}: Can't get DirectoryInfo on the newly copied folder. Something must have gone wrong.");
                return new Result<WebServerFile>(null, StatusCode.SERVER_ERROR);
            }

            return new Result<WebServerFile>(new WebServerFile(newDirectoryInfo), StatusCode.OK);
        }

        public async Task<IServerResult<bool>> MoveFile(FileModel source, FileModel destination)
        {
            var result = await CopyOrMoveValidation(source, destination, false, DirectoryAccessFlags.MoveFile, FileString);
            if (result.Code != StatusCode.OK)
            {
                return new Result<bool>(false, result.Code);
            }

            // Ensure the original file is valid
            try
            {
                var oldFileInfo = new FileInfo(result.SourceFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(MoveFile)}: Can't get FileInfo on the source file ({result.SourceFullPath}). Something must have gone wrong. Possible attack vector");
                return new Result<bool>(false, StatusCode.SERVER_ERROR);
            }

            // Ensure file doesn't exist at destination
            if (!(new FileInfo(result.DestinationFullPath).Exists))
            {
                try
                {
                    File.Move(result.SourceFullPath, result.DestinationFullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(MoveFile)}: Error copying file {result.SourceFullPath} to {result.DestinationFullPath}");
                    return new Result<bool>(false, StatusCode.SERVER_ERROR);
                }
            }
            else
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(MoveFile)}: The file already exists in the destination folder, can't move it there");
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            return new Result<bool>(true);
        }

        public async Task<IServerResult<bool>> MoveFolder(FileModel source, FileModel destination)
        {
            var result = await CopyOrMoveValidation(source, destination, false, DirectoryAccessFlags.MoveFolder, FolderString);
            if (result.Code != StatusCode.OK)
            {
                return new Result<bool>(false, result.Code);
            }

            // Ensure the original folder is valid
            DirectoryInfo oldFolderInfo;
            try
            {
                oldFolderInfo = new DirectoryInfo(result.SourceFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(MoveFolder)}: Can't get DirectoryInfo on the source folder ({result.SourceFullPath}). Something must have gone wrong. Possible attack vector");
                return new Result<bool>(false, StatusCode.SERVER_ERROR);
            }

            try
            {
                oldFolderInfo.MoveTo(result.DestinationFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{this.GetType().Name}] {nameof(MoveFolder)}: Error copying folder {result.SourceFullPath} to {result.DestinationFullPath}");
                return new Result<bool>(false, StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true);
        }

        public async Task<IServerResult<bool>> Delete(FileModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            var dirList = await GetSingleDirectoryList(_user.Id);
            var dir = dirList.Where(x => x.Name == model.Directory).FirstOrDefault();
            if (dir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Delete)}: Invalid folder name provided. Protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<bool>(false, StatusCode.BAD_REQUEST);
            }

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(Path.Combine(dir.Path, model.SubDirectory, model.Name));
            }
            catch
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(Delete)}: Invalid file provided. Protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<bool>(false, StatusCode.UNAUTHORIZED);
            }

            bool isDir = false;
            if (fi.Attributes.HasFlag(FileAttributes.Directory))
            {
                if (!dir.AccessFlags.HasFlag(DirectoryAccessFlags.DeleteFolder))
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(Delete)}: Unable to delete folder. User: " + _user.Id + " @ " + _user.IP + " doesn't have permission to delete this folder.");
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
                isDir = true;
            }
            else
            {
                if (!dir.AccessFlags.HasFlag(DirectoryAccessFlags.DeleteFile))
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(Delete)}: Unable to delete file. User: " + _user.Id + " @ " + _user.IP + " doesn't have permission to delete this file.");
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
            }

            if (!fi.Directory.FullName.StartsWith(dir.Path))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(Delete)}: Path escalation attack attempted by {_user.Id} @ {_user.IP} - Filename={fi.Directory.FullName.ToString()}{Path.DirectorySeparatorChar}{model.Name}!");
                return new Result<bool>(false, StatusCode.FORBIDDEN);
            }


            // File Delete
            if (!isDir && fi.Exists)
            {
                try
                {
                    fi.Delete();
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {nameof(Delete)}: Unauthorized protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
                catch (SecurityException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {nameof(Delete)}: Security exception in protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
                catch (IOException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {nameof(Delete)}: IO exception in protected file deletion attempt by " + _user.Id + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<bool>(false, StatusCode.BAD_REQUEST);
                }
            }
            // Directory Delete
            else if (isDir)
            {
                var di = new DirectoryInfo(fi.FullName);
                try
                {
                    di.Delete();
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {nameof(Delete)}: Unauthorized protected folder deletion attempt by " + _user.Id + " @ " + _user.IP + " - Foldername=" + model.Name);
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
                catch (SecurityException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {nameof(Delete)}: Security exception in protected folder deletion attempt by " + _user.Id + " @ " + _user.IP + " - Foldername=" + model.Name);
                    return new Result<bool>(false, StatusCode.UNAUTHORIZED);
                }
                catch (IOException)
                {
                    _logger.LogError($"[{this.GetType().Name}] {nameof(Delete)}: IO exception in protected folder deletion attempt by " + _user.Id + " @ " + _user.IP + " - Foldername=" + model.Name);
                    return new Result<bool>(false, StatusCode.BAD_REQUEST);
                }
            }
            // File doesn't exist
            else
            {
                return new Result<bool>(true, StatusCode.NO_CONTENT);
            }


            return new Result<bool>(true, StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        private async Task<string> GetMaxRole(WSUser user)
        {
            var roles = await _user.GetUserRolesAsync(user);

            if (roles.Contains(ADMINISTRATOR_STRING))
            {
                return ADMINISTRATOR_STRING;
            }
            else if (roles.Contains(ELEVATED_STRING))
            {
                return ELEVATED_STRING;
            }
            else if (roles.Contains(USER_STRING))
            {
                return USER_STRING;
            }
            else
            {
                return null;
            }
        }

        private Tuple<IEnumerable<string>, string> GetStrippedFilename(string fn)
        {
            var folders = new List<string>();
            var cleaned = string.Copy(fn);

            while (cleaned.Contains('/') || cleaned.Contains('\\'))
            {
                var index = new int[] { cleaned.IndexOf('/'), cleaned.IndexOf('\\') }.Min();
                if (index == -1)
                {
                    index = new int[] { cleaned.IndexOf('/'), cleaned.IndexOf('\\') }.Max();
                }

                if (cleaned.Length == index + 1)
                {
                    _logger.LogWarning($"[{this.GetType().Name}] {nameof(GetStrippedFilename)}: Filename ends with a slash. Invalid filename.");
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
            {
                return null;
            }

            if (dir == null)
            {
                return null;
            }

            string fn = string.Copy(dir);
            if (!dir.Contains(Path.DirectorySeparatorChar))
            {
                fn += Path.DirectorySeparatorChar;
            }

            for (int i = 0; i < folders.Count; i++)
            {
                // Ignore first folder as it is not necessarily a valid value. That is what the dir variable is
                if (i == 0)
                {
                    continue;
                }

                if (fn[fn.Length - 1] != Path.DirectorySeparatorChar)
                {
                    fn += Path.DirectorySeparatorChar;
                }

                fn += folders[i];
            }

            return fn;
        }

        private string GetSourceFile(string dir, string filename)
        {
            // TODO: Add logger stuff for these null checks
            if (dir == null)
            {
                return null;
            }

            if (filename == null)
            {
                return null;
            }

            return Path.Combine(dir, filename);
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            System.IO.Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        private async Task<CopyResult> CopyOrMoveValidation(FileModel source, FileModel destination, bool isCopy, DirectoryAccessFlags flag, string type)
        {
            var dirList = await GetSingleDirectoryList(_user.Id);
            var sourceDir = dirList.Where(x => x.Name == source.Directory).FirstOrDefault();
            var destinationDir = dirList.Where(x => x.Name == destination.Directory).FirstOrDefault();

            var result = new CopyResult();

            if (sourceDir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: Invalid source folder name provided. {type} move attempt by " + _user.Id + " @ " + _user.IP + " - Foldername=" + sourceDir.Name);
                result.Code = StatusCode.BAD_REQUEST;
                return result;
            }

            if (destinationDir == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: Invalid destination folder name provided. {type} move attempt by " + _user.Id + " @ " + _user.IP + " - Foldername=" + destinationDir.Name);
                result.Code = StatusCode.BAD_REQUEST;
                return result;
            }

            if (!sourceDir.AccessFlags.HasFlag(flag))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: {_user.Id} @ {_user.IP} is attempting to move a source {type} that doesn't allow {type} moving");
                result.Code = StatusCode.UNAUTHORIZED;
                return result;
            }

            if (!destinationDir.AccessFlags.HasFlag(flag))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: {_user.Id} @ {_user.IP} is attempting to move a {type} into a destination that doesn't allow {type} moving");
                result.Code = StatusCode.UNAUTHORIZED;
                return result;
            }

            // Folder name cannot change during a move operation. It's name must stay the same, otherwise it's a rename
            if (source.Name != destination.Name)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: Cannot rename a {type} when copying");
                result.Code = StatusCode.BAD_REQUEST;
                return result;
            }

            // If the source directory is the same as the destination directory, this is an invalid move operation. You can't move a folder to the same folder
            // If this is a copy operation, then we can append an underscore and a number to it and place it in the same directory
            if (!isCopy && source.Directory == destination.Directory && source.SubDirectory == destination.SubDirectory)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: New {type} and old {type} are the same. Can't move a {type} into the same {type}");
                result.Code = StatusCode.BAD_REQUEST;
                return result;
            }

            result.SourceBasePath = string.IsNullOrWhiteSpace(source.SubDirectory) ? sourceDir.Path : Path.Combine(sourceDir.Path, source.SubDirectory);
            result.SourceFullPath = Path.Combine(result.SourceBasePath, source.Name);

            result.DestinationBasePath = string.IsNullOrWhiteSpace(destination.SubDirectory) ? destinationDir.Path : Path.Combine(destinationDir.Path, destination.SubDirectory);
            result.DestinationFullPath = Path.Combine(result.DestinationBasePath, destination.Name);

            // Ensure the base directory of the destination actually exists
            if (!System.IO.Directory.Exists(result.DestinationBasePath))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: Destination folder selected doesn't exist. Can't move a {type} to a non-existant folder name of {result.DestinationBasePath}");
                result.Code = StatusCode.BAD_REQUEST;
                return result;
            }

            // Use the platform specific set of invalid characters to ensure the new name will be valid
            var invalidCharacters = Path.GetInvalidPathChars();
            if (invalidCharacters.Any(x => result.DestinationFullPath.Contains(x)))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: New {type} contains invalid characters");
                result.Code = StatusCode.BAD_REQUEST;
                return result;
            }

            // Check for path escalation, and that the source and destination folders start with folder paths that are accessible to this user
            if (IsPathEscalation(result.DestinationFullPath) || !result.SourceFullPath.StartsWith(sourceDir.Path) || !result.DestinationFullPath.StartsWith(destinationDir.Path))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(CopyOrMoveValidation)}: Looks like a path escalation attack from user ({_user.Id}) for source path: {result.SourceFullPath} and destination path: {result.DestinationFullPath}");
                result.Code = StatusCode.FORBIDDEN;
                return result;
            }

            result.Code = StatusCode.OK;
            return result;
        }

        private string HandleSameNameCopy(CopyResult result, FileInfo oldFileInfo)
        {
            string newCopyString = null;
            if (new FileInfo(result.DestinationFullPath).Exists || new DirectoryInfo(result.DestinationFullPath).Exists)
            {
                var fn = oldFileInfo.Name.Substring(0, oldFileInfo.Name.Length - oldFileInfo.Extension.Length);
                var newFn = (fn + " - Copy");
                newCopyString = newFn + oldFileInfo.Extension;
                var fileExists = true;
                var increment = 1;
                while (fileExists)
                {
                    if (new FileInfo(Path.Combine(result.DestinationBasePath, newCopyString)).Exists || new DirectoryInfo(Path.Combine(result.DestinationBasePath, newCopyString)).Exists)
                    {
                        if (increment == 1)
                        {
                            newFn += $" ({++increment})";
                            newCopyString = newFn + oldFileInfo.Extension;
                        }
                        else
                        {
                            newCopyString = newCopyString.Replace($" - Copy ({increment++})", $" - Copy ({increment})");
                        }
                    }
                    else
                    {
                        fileExists = false;
                    }
                }
            }

            return newCopyString;
        }

        // TODO: Re-evaluate whether to use something like this
        private void IntelligentCopy()
        {
            //if (Regex.IsMatch(fn, $".*({CopyString}).*"))
            //{
            //    var end = fn.Substring(fn.LastIndexOf(CopyString) + CopyString.Length);
            //    if (end.Length == 0)
            //    {
            //        // This means there is only 1 copy so far, so we need to add the numbering
            //        var newFn = (fn + " - Copy");
            //        newCopyString = newFn + oldFileInfo.Extension;

            //        var fileExists = true;
            //        var increment = 1;
            //        while (fileExists)
            //        {
            //            if (new FileInfo(Path.Combine(result.DestinationBasePath, newCopyString)).Exists)
            //            {
            //                if (increment == 1)
            //                {
            //                    newFn += $" ({++increment})";
            //                    newCopyString = newFn + oldFileInfo.Extension;
            //                }
            //                else
            //                {
            //                    newCopyString = newCopyString.Replace($" - Copy ({increment++})", $" - Copy ({increment})");
            //                }
            //            }
            //            else
            //            {
            //                fileExists = false;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (Regex.IsMatch(end, @" \([0-9]*\)"))
            //        {
            //            var openBraceIndex = end.IndexOf('(');
            //            var closeBraceIndex = end.IndexOf(')');
            //            var numStr = end.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1);
            //            if (int.TryParse(numStr, out int number))
            //            {
            //                var incremented = number++;
            //                var newEnd = end.Replace(numStr, incremented.ToString());

            //                var baseStr = fn.Substring(0, fn.LastIndexOf(" ("));
            //                newCopyString = baseStr + newEnd + oldFileInfo.Extension;
            //            }
            //        }
            //    }
            //}
        }

        private async Task<IEnumerable<ServerUserDirectory>> GetServerUserDirectoryList(WSUser user)
        {
            if (user == null)
            {
                return new List<ServerUserDirectory>();
            }

            return await _context.ServerUserDirectory.Where(x => x.UserId == user.Id).ToListAsync();
        }

        private async Task<IEnumerable<ServerGroupDirectory>> GetServerGroupDirectoryList(string role)
        {
            if (role == null)
            {
                return new List<ServerGroupDirectory>();
            }

            return await _context.ServerGroupDirectory.Where(x => x.Role == role).ToListAsync();
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

        private bool IsPathEscalation(string path)
        {
            return path.Contains("..") ? true : false;
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

            var mediaInfo = await FFmpeg.GetMediaInfo(file.FullName);
            var videoStream = mediaInfo.VideoStreams.First();
            var audioStream = mediaInfo.AudioStreams.First();

            //Change some parameters of video stream
            videoStream
                //Set size to 480p
                .SetSize(VideoSize.Hd480)
                //Set codec which will be used to encode file. If not set it's set automatically according to output file extension
                .SetCodec(VideoCodec.h264);

            var conversion = new Conversion()
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

            await Console.Out.WriteLineAsync($"[{this.GetType().Name}] {nameof(ConvertFile)}: Finished converion file [{file.Name}]");
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
