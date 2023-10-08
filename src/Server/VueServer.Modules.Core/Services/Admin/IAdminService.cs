using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Models;

namespace VueServer.Modules.Core.Services.Admin
{
    public interface IAdminService
    {
        Task<IServerResult<bool>> SetServerSetting(ServerSettings setting);
        Task<IServerResult<bool>> DeleteServerSetting(string key);
    }
}
