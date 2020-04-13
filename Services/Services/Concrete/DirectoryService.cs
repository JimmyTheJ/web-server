using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using VueServer.Domain;
using VueServer.Domain.Enums;
using VueServer.Domain.Helper;
using VueServer.Domain.Interface;
using VueServer.Domain.Concrete;
using VueServer.Models;
using VueServer.Models.Directory;
using VueServer.Services.Interface;

using Xabe.FFmpeg;
using Xabe.FFmpeg.Enums;
using Microsoft.AspNetCore.Http;
using VueServer.Models.Request;

namespace VueServer.Services.Concrete
{
    public class DirectoryService : IDirectoryService
    {
        private ILogger _logger { get; set; }

        private IUserService _user { get; set; }

        private IWebHostEnvironment _env { get; set; }

        private IConfiguration _config { get; set; }
         
        public DirectoryService(ILoggerFactory logger, IUserService user, IWebHostEnvironment env, IConfigurationRoot config)
        {
            _logger = logger?.CreateLogger<DirectoryService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _env = env ?? throw new ArgumentNullException("Hosting environment null");
            _config = config ?? throw new ArgumentNullException("Configuration null");
        }

        # region -> Public Functions

        public IResult<IEnumerable<ServerDirectory>> GetDirectories ()
        {
            var dirs = GetSingleDirectoryList(true);
            return new Result<IEnumerable<ServerDirectory>>(dirs, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Tuple<string, string, string>>> Download (string filename, bool media = false)
        { 
            if (string.IsNullOrWhiteSpace(filename))
            {
                _logger.LogWarning("Directory.Download: Filename passed is null or empty");
                return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var tuple = GetStrippedFilename(filename);
            if (tuple == null)
            {
                _logger.LogWarning("Directory.Download: Error stripping filename. Null Tuple object");
                return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1 == null)
            {
                _logger.LogWarning("Directory.Download: Error stripping filename. Null Tuple.Item1");
                return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (tuple.Item2 == null)
            {
                _logger.LogWarning("Directory.Download: Error stripping filename. Null Tuple.Item2");
                return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (tuple.Item1.Count() == 0)
            {
                _logger.LogWarning("Directory.Download: Filename contains no folders. Invalid filename");
                return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var list = GetSingleDirectoryList().ToList();
            var folders = tuple.Item1.ToList();

            var baseDir = list.Where(a => a.Name == folders[0]).Select(a => a.Path).FirstOrDefault();
            if (baseDir == null)
            {
                _logger.LogWarning($"Directory.Download: Invalid folder name: {folders[0]}");
                return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var cleanFilename = tuple.Item2;
            var sourcePath = GetSourcePath(baseDir, folders);
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
                        return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (PathTooLongException)
                    {
                        _logger.LogError("DownloadProtectedFile: PathTooLongException");
                        return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        _logger.LogError("DownloadProtectedFile: DirectoryNotFoundException");
                        return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (IOException e)
                    {
                        _logger.LogError("DownloadProtectedFile: IOException");
                        _logger.LogError(e.StackTrace);
                        return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.LogError("DownloadProtectedFile: UnauthorizedException");
                        return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                    }
                    catch (NotSupportedException)
                    {
                        _logger.LogError("DownloadProtectedFile: NotSupportedException");
                        return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
                    }

                    _logger.LogInformation("Private zip archive download begun by " + _user.Name + " @ " + _user.IP + " - name=" + filename);
                    return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(zipPath, contentType, cleanFilename), Domain.Enums.StatusCode.OK);
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
                _logger.LogError("DownloadProtectedFile: Unknown exception accessing file");
                return new Result<Tuple<string, string, string>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            _logger.LogInformation("Private download begun by " + _user.Name + " @ " + _user.IP + " - name=" + filename);
            //if (!media)
                return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(Path.Combine(sourcePath, cleanFilename), contentType, cleanFilename), StatusCode.OK);
            //else
            //    return new Result<Tuple<string, string, string>>(new Tuple<string, string, string>(mediaFilename.Item1, mediaFilename.Item2, cleanFilename), StatusCode.OK);
        }

        public IResult<IOrderedEnumerable<WebServerFile>> Load (string directory, string subDir)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                _logger.LogWarning("Directory.Load: Directory is null. Can't get list.");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var list = GetSingleDirectoryList().ToList();
            var basePath = list.Where(a => a.Name == directory).Select(a => a.Path).FirstOrDefault();
            if (basePath == null)
            {
                _logger.LogWarning($"Directory.Load: Invalid folder name: {directory}");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            string fullPath = string.Empty;
            Uri uri = null;

            if (!string.IsNullOrWhiteSpace(subDir))
            {
                fullPath = Path.Combine(basePath, subDir);
                // Security check to ensure the user isn't trying a path escalation attack
                try
                {
                    uri = new Uri(fullPath);
                    if (!uri.AbsolutePath.ToLower().StartsWith(basePath.ToLower()) && !uri.LocalPath.ToLower().StartsWith(basePath.ToLower()))
                    {
                        _logger.LogWarning($"{_user.Name} @ {_user.IP} is trying to do a path escalation attack!");
                        return new Result<IOrderedEnumerable<WebServerFile>>(null, Domain.Enums.StatusCode.FORBIDDEN);
                    }
                }
                catch
                {
                    _logger.LogInformation($"{_user.Name} @ {_user.IP} sent malformed URI path in sub directory.");
                    return new Result<IOrderedEnumerable<WebServerFile>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
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
                files = dir.GetFileSystemInfos();
            }
            catch (Exception)
            {
                _logger.LogWarning($"Directory.Load: Error getting directory or file system info at: {basePath}");
                return new Result<IOrderedEnumerable<WebServerFile>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var fileList = new List<WebServerFile>();
            foreach (var file in files)
            {
                fileList.Add(new WebServerFile(file));
            }

            return new Result<IOrderedEnumerable<WebServerFile>>(fileList.OrderByDescending(x => x.IsFolder).ThenBy(x => x.Title), Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult> Upload(UploadFileRequest model)
        {
            var uploadDirs = GetSingleDirectoryList();
            string baseSaveDir = uploadDirs.Where(x => x.Name == model.Directory).Select(y => y.Path).FirstOrDefault();
            string saveDir = baseSaveDir;

            if (!string.IsNullOrEmpty(model.SubDirectory))
            {
                saveDir += model.SubDirectory;
            }

            _logger.LogInformation("Upload started from " + _user.Name + " @ " + _user.IP + " - Filename=" + model.File.FileName);

            if (!Directory.Exists(saveDir))
            {
                _logger.LogInformation($"{_user.Name} @ {_user.IP} is attempting to create a folder while uploading a file");
                return new Result<IResult>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            try
            {
                var uri = new Uri(saveDir);
                if (!uri.LocalPath.StartsWith(baseSaveDir))
                {
                    _logger.LogWarning($"{_user.Name} @ {_user.IP} is trying to do a path escalation attack!");
                    return new Result<IResult>(null, Domain.Enums.StatusCode.FORBIDDEN);
                }
            }
            catch
            {
                _logger.LogInformation($"{_user.Name} @ {_user.IP} sent malformed URI path.");
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            try
            {
                using (FileStream fs = File.Create(saveDir + "\\" + model.File.FileName))
                {
                    await model.File.CopyToAsync(fs);
                    fs.Flush();
                    _logger.LogInformation("Upload SUCCESS from " + _user.Name + " @ " + _user.IP + " - Filename=" + model.File.FileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to write file dir\n" + e.StackTrace);
                _logger.LogInformation("Upload FAILED from " + _user.Name + " @ " + _user.IP + " - Filename=" + model.File.FileName);
                return new Result<IResult>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }

        public IResult Delete(DeleteFileModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);

            var uploadDirs = GetSingleDirectoryList();
            string dir = uploadDirs.Where(x => x.Name == model.Directory).Select(y => y.Path).FirstOrDefault();
            if (dir == null)
            {
                _logger.LogInformation("Invalid folder name provided. Protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(Path.Combine(dir, model.SubDirectory, model.Name));
            }
            catch
            {
                _logger.LogInformation("Invalid file provided. Protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Name);
                return new Result<IResult>(null, Domain.Enums.StatusCode.UNAUTHORIZED);
            }

            if (!fi.Directory.FullName.StartsWith(dir))
            {
                _logger.LogWarning($"Path escalation attack attempted by {_user.Name} @ {_user.IP} - Filename={fi.Directory.FullName.ToString()}\\{model.Name}!");
                return new Result<IResult>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            if (fi.Exists)
            {
                try
                {
                    fi.Delete();
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogError("Unauthorized protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<IResult>(null, Domain.Enums.StatusCode.UNAUTHORIZED);
                }
                catch (SecurityException)
                {
                    _logger.LogError("Security exception in protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<IResult>(null, Domain.Enums.StatusCode.UNAUTHORIZED);
                }
                catch (IOException)
                {
                    _logger.LogError("IO exception in protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Name);
                    return new Result<IResult>(null, Domain.Enums.StatusCode.BAD_REQUEST);
                }
            }


            return new Result<IResult>(null, Domain.Enums.StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        private ACCESS_LEVELS GetMaxLevel ()
        {
            if (_user.Context.User.IsInRole(Constants.ADMINISTRATOR_STRING) )
                return ACCESS_LEVELS.ADMIN;
            else if (_user.Context.User.IsInRole(Constants.ELEVATED_STRING) )
                return ACCESS_LEVELS.ELEVATED;
            else if (_user.Context.User.IsInRole(Constants.USER_STRING) )
                return ACCESS_LEVELS.GENERAL;
            else
                return 0;
        }

        private string GetMaxLevelString ()
        {
            if (_user.Context.User.IsInRole(Constants.ADMINISTRATOR_STRING) )
                return Constants.ADMINISTRATOR_STRING;
            else if (_user.Context.User.IsInRole(Constants.ELEVATED_STRING) )
                return Constants.ELEVATED_STRING;
            else if (_user.Context.User.IsInRole(Constants.USER_STRING) )
                return Constants.USER_STRING;
            else
                return Constants.INVALID_STRING;
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

        private ServerDirectoryLists GetServerGroupDirectoryLists()
        {
            ServerDirectoryLists dirs = new ServerDirectoryLists
            {
                Admin = GetServerDirectoryList("Directories:Group:Admin"),
                Elevated = GetServerDirectoryList("Directories:Group:Elevated"),
                General = GetServerDirectoryList("Directories:Group:General"),
                User = GetServerDirectoryList($"Directories:User:{_user.Name}"),
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
                    list.Add(new ServerDirectory(s.GetValue<string>("Name"), s.GetValue<string>("Path"), s.GetValue<bool?>("Default") ?? false));
                }
            }
            catch (Exception)
            {
                _logger.LogWarning("Error deserializing directories from appsettings json");
            }

            return list;
        }

        private IList<ServerDirectory> GetSingleDirectoryList(bool stripPath = false)
        {
            var dirs = GetServerGroupDirectoryLists();
            switch (GetMaxLevel())
            {
                case ACCESS_LEVELS.ADMIN:
                    return GetFullUserFolderList(dirs.Admin, dirs.User, stripPath);
                case ACCESS_LEVELS.ELEVATED:
                    return GetFullUserFolderList(dirs.Elevated, dirs.User, stripPath);
                case ACCESS_LEVELS.GENERAL:
                    return GetFullUserFolderList(dirs.General, dirs.User, stripPath);
                default:
                    return new List<ServerDirectory>();
            }
        }

        private IList<ServerDirectory> GetFullUserFolderList(IList<ServerDirectory> groupList, IList<ServerDirectory> userList, bool stripPath)
        {
            if (groupList == null)
            {
                _logger.LogWarning("DirectoryService.GetFullUserFolderList: groupList is null returning null");
                return null;
            }

            IList<ServerDirectory> fullList = new List<ServerDirectory>();

            foreach (var item in groupList)
            {
                if (stripPath)
                    item.Path = null;
                fullList.Add(item);
            }

            if (userList == null)
            {
                _logger.LogWarning("DirectoryService.GetFullUserFolderList: userList is null returning just groupList values");
                return fullList;
            }

            foreach (var item in userList)
            {
                if (stripPath)
                    item.Path = null;
                fullList.Add(item);
            }

            return fullList;

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

            await Console.Out.WriteLineAsync($"Finished converion file [{file.Name}]");
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
