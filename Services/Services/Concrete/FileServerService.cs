using Microsoft.Extensions.Logging;

using System;
using System.IO;

using VueServer.Common.Interface;
using VueServer.Common.Concrete;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class FileServerService : IFileServerService
    {
        private ILogger _logger { get; set; }

        private IUserService _user { get; set; }

        public FileServerService (ILoggerFactory logger, IUserService user)
        {
            _logger = logger.CreateLogger<FileServerService>();
            _user = user;
        }

        public IResult<string[]> GetFiles (string webrootPath)
        {
            var folder = @"\videos";
            try
            {
                var files = Directory.GetFiles(webrootPath + folder);

                for (var i = 0; i < files.Length; i++)
                {
                    //files[i] = files[i].Replace(baseDir, "");
                    files[i] = Path.GetFileName(files[i]);
                }

                return new Result<string[]>(files, Common.Enums.StatusCode.OK);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error trying to get all the files in the ${ folder } folder");
            }

            return new Result<string[]>(null, Common.Enums.StatusCode.NO_CONTENT);
        }
    }
}
