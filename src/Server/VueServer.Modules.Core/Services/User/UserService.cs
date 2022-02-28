using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Services.User
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _httpContext { get; set; }

        private UserManager<WSUser> _userManager { get; set; }

        private RoleManager<WSRole> _roleManager { get; set; }

        public UserService() { }

        public UserService(
            IHttpContextAccessor httpContext,
            UserManager<WSUser> userManager,
            RoleManager<WSRole> roleManager)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException("HttpContextAccessor is null");
            _userManager = userManager ?? throw new ArgumentNullException("User manager is null");
            _roleManager = roleManager ?? throw new ArgumentNullException("Role manager is null");
        }

        public HttpContext Context => _httpContext.HttpContext;

        public string Id => _httpContext.HttpContext.User.Claims
                    .Where(a => a.Type == JwtRegisteredClaimNames.Sub
                        || a.Type == @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")
                    ?.Select(a => a.Value)?.SingleOrDefault()
                    ?? throw new Exception("Can't get username from User Claims.");

        public string IP => _httpContext.HttpContext.Connection.RemoteIpAddress.ToString();

        public Task<WSUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(_httpContext.HttpContext.User);
        }

        public Task<WSUser> GetUserByNameAsync(string name)
        {
            return _userManager.FindByNameAsync(name);
        }

        public Task<WSUser> GetUserByIdAsync(string id)
        {
            return _userManager.FindByIdAsync(id);
        }

        public Task<IList<string>> GetUserRolesAsync(WSUser user)
        {
            return _userManager.GetRolesAsync(user);
        }

        public Task<WSRole> GetUserRoleByNameAsync(string name)
        {
            return _roleManager.FindByNameAsync(name);
        }
    }
}
