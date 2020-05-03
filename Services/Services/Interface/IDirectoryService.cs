using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using VueServer.Domain.Enums;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Request;
using VueServer.Models.Directory;


namespace VueServer.Services.Interface
{
    public interface IDirectoryService
    {
        IResult<IEnumerable<ServerDirectory>> GetDirectories ();

        Task<IResult<Tuple<string, string, string>>> Download(string filename);

        Task<IResult<Tuple<string, string, long>>> StreamMedia(string filename, long start, long end);

        IResult<IOrderedEnumerable<WebServerFile>> Load (string directory, string subDir);

        Task<IResult> Upload(UploadFileRequest model);

        IResult Delete(DeleteFileModel model);
    }
}
