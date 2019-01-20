using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using System.Linq;
using System.Threading.Tasks;

using VueServer.Models.Account;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _httpContext { get; set; }

        private UserManager<ServerIdentity> _userManager { get; set; }

        public UserService() {}

        public UserService (
            IHttpContextAccessor httpContext,
            UserManager<ServerIdentity> userManager)
        {
            _httpContext = httpContext;
            _userManager = userManager;
        }

        public HttpContext Context {
            get {
                return _httpContext.HttpContext;
            }
        }

        public string Name  { 
            get {
                return _httpContext.HttpContext.User.Claims.Where(a => a.Type == "sub").Select(a => a.Value).FirstOrDefault();
            }
        }

        public string IP {
            get {
                return _httpContext.HttpContext.Connection.RemoteIpAddress.ToString();
            }
        }

        //public HttpContext GetContext() => _httpContext.HttpContext;

        //public string GetUserName() => _httpContext.HttpContext.User.Identity.Name;

        //public string GetUserIP() => _httpContext.HttpContext.Connection.RemoteIpAddress.ToString();

        //public string GetUsername() => _userManager.GetUserName(_httpContext.HttpContext.User);

        //public string GetUsername()
        //{
        //    string username = string.Empty;
        //    var claims = _httpContext.HttpContext.User.Claims;
        //    foreach (var claim in claims)
        //    {
        //        if (claim.Type == "sub")
        //        {
        //            username = claim.Value;
        //            break;
        //        }
                
        //    }

        //    return username;
        //}

        //public Task<ServerIdentity> GetUserAsync()
        //{
        //    _userManager.Get
        //}

        public Task<ServerIdentity> GetUserAsync() => _userManager.GetUserAsync(_httpContext.HttpContext.User);
    }
}
