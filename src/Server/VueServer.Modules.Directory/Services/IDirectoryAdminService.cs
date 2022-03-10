using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Models;
using VueServer.Modules.Directory.Models;

namespace VueServer.Modules.Directory.Services
{
    public interface IDirectoryAdminService
    {
        Task<IResult<IEnumerable<ServerSettings>>> GetDirectorySettings();
        Task<IResult<IEnumerable<ServerGroupDirectory>>> GetGroupDirectories();
        Task<IResult<IEnumerable<ServerUserDirectory>>> GetUserDirectories();
        Task<IResult<int>> AddGroupDirectory(ServerGroupDirectory dir);
        Task<IResult<long>> AddUserDirectory(ServerUserDirectory dir);
        Task<IResult<bool>> DeleteGroupDirectory(int id);
        Task<IResult<bool>> DeleteUserDirectory(long id);
        Task<bool> CreateDefaultFolder(string username);
    }
}
