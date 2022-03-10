using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Core.Models;

namespace VueServer.Modules.Core.Services.Admin
{
    public interface IAdminService
    {
        Task<IResult<bool>> SetServerSetting(ServerSettings setting);
        Task<IResult<bool>> DeleteServerSetting(string key);
    }
}
