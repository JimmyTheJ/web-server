using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using VueServer.Common.Interface;
using VueServer.Common.Concrete;
using VueServer.Models;
using static VueServer.Common.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using VueServer.Services.Interface;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VueServer.Models.Directory;

namespace VueServer.Services.Concrete
{
    public class UploadService : IUploadService
    {
        private ILogger _logger { get; set; }

        private IUserService _user { get; set; }

        private IConfiguration _config { get; set; }

        public UploadService (ILoggerFactory logger, IUserService user, IConfiguration config)
        {
            _logger = logger.CreateLogger<UploadService>() ?? throw new ArgumentNullException("Logger null");
            _user = user ?? throw new ArgumentNullException("User service null");
            _config = config ?? throw new ArgumentNullException("Configuration null");
        }

        public IResult<IEnumerable<string>> GetFolderList ()
        {
            var uploadDirs = GetUploadDirectories();

            return new Result<IEnumerable<string>>(uploadDirs.Select(x => x.Name), Common.Enums.StatusCode.OK);
        }

        public IResult<IEnumerable<DirectoryContentsResponse>> GetFiles ()
        {
            var uploadDirs = GetUploadDirectories();

            var responseList = new List<DirectoryContentsResponse>();

            
            foreach (var item in uploadDirs)
            {
                var folderName = item.Name;
                string[] folderFiles = null;
                try
                {
                    folderFiles = Directory.GetFiles(item.Path);
                }
                catch (Exception)
                {
                    _logger.LogError($"Upload.GetFiles: Error building a file/folder list at '{item.Name}'.");
                    return new Result<IEnumerable<DirectoryContentsResponse>>(new List<DirectoryContentsResponse>(), Common.Enums.StatusCode.SERVER_ERROR);
                }

                var trimmedFiles = new List<string>();
                foreach (var file in folderFiles)
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        if (file.Contains('/'))
                        {
                            trimmedFiles.Add(file.Substring(file.LastIndexOf('/') + 1));
                        }
                    }
                    else
                    {
                        if (file.Contains('\\'))
                        {
                            trimmedFiles.Add(file.Substring(file.LastIndexOf('\\') + 1));
                        }
                    }
                }

                responseList.Add(new DirectoryContentsResponse() { Folder = folderName, Files = trimmedFiles });
            };

            return new Result<IEnumerable<DirectoryContentsResponse>>(responseList, Common.Enums.StatusCode.OK);
        }

        public async Task<IResult> Upload (UploadFileRequest model)
        {
            var uploadDirs = GetUploadDirectories();
            string saveDir = uploadDirs.Where(x => x.Name == model.Name).Select(y => y.Path).FirstOrDefault();
            _logger.LogInformation("Upload started from " + _user.Name + " @ " + _user.IP + " - Filename=" + model.File.FileName);

            if (!Directory.Exists(saveDir))
            {
                Console.WriteLine("Directory doesn't exist. Creating it!");

                try
                {
                    Directory.CreateDirectory(saveDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to create dir(s)\n" + e.StackTrace);
                    return new Result<IResult>(null, Common.Enums.StatusCode.SERVER_ERROR);
                }
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
                return new Result<IResult>(null, Common.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<IResult>(null, Common.Enums.StatusCode.OK);
        }

        public IResult Delete (DeleteFileModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Filename))
                return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);

            var uploadDirs = GetUploadDirectories();
            string dir = uploadDirs.Where(x => x.Name == model.Folder).Select(y => y.Path).FirstOrDefault();
            if (dir == null) {
                _logger.LogWarning("Invalid folder name provided. Protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Filename);
                return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);
            }

            FileInfo fi = new FileInfo(Path.Combine(dir, model.Filename));
            if (fi.Exists) {
                try
                {
                    fi.Delete();
                }
                catch(UnauthorizedAccessException)
                {
                    _logger.LogWarning("Unauthorized protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Filename);
                    return new Result<IResult>(null, Common.Enums.StatusCode.UNAUTHORIZED);
                }
                catch (SecurityException) {
                    _logger.LogWarning("Security exception in protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Filename);
                    return new Result<IResult>(null, Common.Enums.StatusCode.UNAUTHORIZED);
                }
                catch(IOException)
                {
                    _logger.LogWarning("IO exception in protected file deletion attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + model.Filename);
                    return new Result<IResult>(null, Common.Enums.StatusCode.BAD_REQUEST);
                }
            }

            return new Result<IResult>(null, Common.Enums.StatusCode.OK);
        }

        private IList<ServerDirectory> GetUploadDirectories ()
        {
            IList<ServerDirectory> uploadList = new List<ServerDirectory>();
            var uploadSection = _config.GetSection("Upload");
            foreach (IConfigurationSection section in uploadSection.GetChildren())
            {
                uploadList.Add(new ServerDirectory(section.GetValue<string>("Name"), section.GetValue<string>("Path")));
            }

            return uploadList;
        }
    }
}
