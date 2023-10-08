using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Directory.Models;

namespace VueServer.Modules.Directory.Services
{
    public interface IDirectoryAdminService
    {
        Task<IServerResult<IEnumerable<ServerSettings>>> GetDirectorySettings();
        Task<IServerResult<IEnumerable<ServerGroupDirectory>>> GetGroupDirectories();
        Task<IServerResult<IEnumerable<ServerUserDirectory>>> GetUserDirectories();
        Task<IServerResult<int>> AddGroupDirectory(ServerGroupDirectory dir);
        Task<IServerResult<long>> AddUserDirectory(ServerUserDirectory dir);
        Task<IServerResult<bool>> DeleteGroupDirectory(int id);
        Task<IServerResult<bool>> DeleteUserDirectory(long id);
        Task<IServerResult<bool>> CreateDefaultFolder(string username);
    }
}
