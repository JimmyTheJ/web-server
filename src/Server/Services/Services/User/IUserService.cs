using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Models.User;

namespace VueServer.Services.User
{
    public interface IUserService
    {
        HttpContext Context { get; }

        string Id { get; }
        string IP { get; }

        Task<WSUser> GetCurrentUserAsync();
        Task<WSUser> GetUserByNameAsync(string name);
        Task<WSUser> GetUserByIdAsync(string id);
        Task<IList<string>> GetUserRolesAsync(WSUser user);
        Task<WSRole> GetUserRoleByNameAsync(string name);
    }
}
