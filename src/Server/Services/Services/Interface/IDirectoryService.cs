using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Request;
using VueServer.Models.Response;

namespace VueServer.Services.Interface
{
    public interface IDirectoryService
    {
        Task<IResult<IEnumerable<ServerDirectory>>> GetDirectories();
        Task<IResult<Tuple<string, string, string>>> Download(string filename, string user, bool media = false);
        Task<IResult<IOrderedEnumerable<WebServerFile>>> Load(string directory, string subDir);
        Task<IResult<WebServerFile>> CreateFolder(string directory, string subDir, string newFolder);
        Task<IResult<WebServerFile>> RenameFile(MoveFileRequest model);
        Task<IResult<WebServerFile>> RenameFolder(MoveFileRequest model);
        Task<IResult<WebServerFile>> Upload(UploadDirectoryFileRequest model);
        Task<IResult<WebServerFile>> CopyFile(FileModel source, FileModel destination);
        Task<IResult<WebServerFile>> CopyFolder(FileModel source, FileModel destination);
        Task<IResult<bool>> MoveFile(FileModel source, FileModel destination);
        Task<IResult<bool>> MoveFolder(FileModel source, FileModel destination);
        Task<IResult<bool>> Delete(FileModel model);
    }
}
