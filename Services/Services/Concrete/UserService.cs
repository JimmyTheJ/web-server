using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

using VueServer.Models.Account;
using VueServer.Models.User;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _httpContext { get; set; }

        private UserManager<WSUser> _userManager { get; set; }

        public UserService() {}

        public UserService (
            IHttpContextAccessor httpContext,
            UserManager<WSUser> userManager)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException("HttpContextAccessor is null");
            _userManager = userManager ?? throw new ArgumentNullException("User manager is null");
        }

        public HttpContext Context {
            get {
                return _httpContext.HttpContext;
            }
        }

        public string Name  { 
            get {
                return _httpContext.HttpContext.User.Claims
                    .Where(a => a.Type == JwtRegisteredClaimNames.Sub 
                        || a.Type == @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    .Select(a => a.Value).FirstOrDefault();
            }
        }

        public string IP {
            get {
                return _httpContext.HttpContext.Connection.RemoteIpAddress.ToString();
            }
        }

        public Task<WSUser> GetUserAsync() => _userManager.GetUserAsync(_httpContext.HttpContext.User);
    }
}
