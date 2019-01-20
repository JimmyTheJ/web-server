using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using VueServer.Models.Account;

namespace VueServer.Services.Interface
{
    public interface IUserService
    {
        HttpContext Context { get; }

        string Name { get; }

        string IP { get; }

        //HttpContext GetContext();

        //string GetUserName();

        //string GetUserIP();

        //string GetUsername();

        Task<ServerIdentity> GetUserAsync();
    }
}
