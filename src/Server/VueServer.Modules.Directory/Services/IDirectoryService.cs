using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Directory.Models;
using VueServer.Modules.Directory.Models.Request;
using VueServer.Modules.Directory.Models.Response;

namespace VueServer.Modules.Directory.Services
{
    public interface IDirectoryService
    {
        Task<IServerResult<IEnumerable<ServerDirectory>>> GetDirectories();
        Task<IServerResult<Tuple<string, string, string>>> Download(string filename, string user, bool media = false);
        Task<IServerResult<IOrderedEnumerable<WebServerFile>>> Load(string directory, string subDir);
        Task<IServerResult<WebServerFile>> CreateFolder(string directory, string subDir, string newFolder);
        Task<IServerResult<WebServerFile>> RenameFile(MoveFileRequest model);
        Task<IServerResult<WebServerFile>> RenameFolder(MoveFileRequest model);
        Task<IServerResult<WebServerFile>> Upload(UploadDirectoryFileRequest model);
        Task<IServerResult<WebServerFile>> CopyFile(FileModel source, FileModel destination);
        Task<IServerResult<WebServerFile>> CopyFolder(FileModel source, FileModel destination);
        Task<IServerResult<bool>> MoveFile(FileModel source, FileModel destination);
        Task<IServerResult<bool>> MoveFolder(FileModel source, FileModel destination);
        Task<IServerResult<bool>> Delete(FileModel model);
    }
}
