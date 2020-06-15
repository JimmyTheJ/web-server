using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using VueServer.Models.User;

namespace VueServer.Services.Interface
{
    public interface IUserService
    {
        HttpContext Context { get; }

        string Name { get; }
        string Id { get;  }
        string IP { get; }

        Task<WSUser> GetUserAsync();
    }
}
