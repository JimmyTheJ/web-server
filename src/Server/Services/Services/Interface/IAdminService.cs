using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Models;
using VueServer.Models.Directory;

namespace VueServer.Services.Interface
{
    public interface IAdminService
    {
        Task<IResult<IEnumerable<ServerSettings>>> GetDirectorySettings();
        Task<IResult<bool>> SetServerSetting(ServerSettings setting);
        Task<IResult<bool>> DeleteServerSetting(string key);
        Task<IResult<IEnumerable<ServerGroupDirectory>>> GetGroupDirectories();
        Task<IResult<IEnumerable<ServerUserDirectory>>> GetUserDirectories();
        Task<IResult<int>> AddGroupDirectory(ServerGroupDirectory dir);
        Task<IResult<long>> AddUserDirectory(ServerUserDirectory dir);
        Task<IResult<bool>> DeleteGroupDirectory(int id);
        Task<IResult<bool>> DeleteUserDirectory(long id);
    }
}
