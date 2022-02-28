using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Domain;
using VueServer.Modules.Core.Models.Account;
using VueServer.Modules.Core.Models.Request;
using VueServer.Modules.Core.Services.Account;
using static VueServer.Domain.DomainConstants.Authentication;
using Route = VueServer.Modules.Core.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Modules.Core.Controllers
{
    [Route(Route.Account.Controller)]
    public class AccountController : Controller
    {
        private readonly IAccountService _service;

        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        // Initializes all components to be able to interface with the Identity Framework
        // To allow users to log in, register, and logout, register roles, etc.
        public AccountController(
            IAccountService service,
            IStatusCodeFactory<IActionResult> factory
        )
        {
            _service = service ?? throw new ArgumentNullException("Account service is null");
            _codeFactory = factory ?? throw new ArgumentNullException("Code factory is null");
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ELEVATED_USER)]
        [Route(Route.Account.UserChangePassword)]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.ChangePassword(model, false));
        }

        /// <summary>
        /// Registering a new user into the Identity Framework
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Account.Register)]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Register(model));
        }

        /// <summary>
        /// Login to a existing user account using the Identity Framework
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Route.Account.Login)]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var result = await _service.Login(HttpContext, model);
            if (result.Obj == null)
            {
                if (HttpContext.Request.Cookies.ContainsKey(DomainConstants.Authentication.REFRESH_TOKEN_COOKIE_KEY))
                {
                    HttpContext.Response.Cookies.Delete(DomainConstants.Authentication.REFRESH_TOKEN_COOKIE_KEY);
                }
            }
            else
            {
                HttpContext.Response.Cookies.Append(result.Obj.RefreshCookie.Key, result.Obj.RefreshCookie.Value, result.Obj.RefreshCookie.Options);
                if (result.Obj.ClientIdCookie != null)
                {
                    HttpContext.Response.Cookies.Append(result.Obj.ClientIdCookie.Key, result.Obj.ClientIdCookie.Value, result.Obj.ClientIdCookie.Options);
                }
            }

            return _codeFactory.GetStatusCode(result);
        }

        /// <summary>
        /// Logout of your account
        /// Authorize checks and sees if the user is logged in to the Identity Framework
        /// All of the null checks and other stuff is done already for you using this attribute
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route(Route.Account.Logout)]
        public async Task<IActionResult> Logout([FromBody] string username)
        {
            HttpContext.Response.Cookies.Delete(DomainConstants.Authentication.REFRESH_TOKEN_COOKIE_KEY);
            return _codeFactory.GetStatusCode(await _service.Logout(HttpContext, username));
        }

        /// <summary>
        /// Refresh the user's JWT token
        /// </summary>
        /// <param name="accessToken">Old token to be refreshed</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Route.Account.RefreshJwt)]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            return _codeFactory.GetStatusCode(await _service.RefreshJwtToken(token, HttpContext.Request.Cookies));
        }

        /// <summary>
        /// Validate JWT and see whether a new JWT is required or not or if the refresh token exists or is expired
        /// /// </summary>
        /// <param name="accessToken">Old token to be validated</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Route.Account.ValidateToken)]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            return _codeFactory.GetStatusCode(await _service.ValidateToken(token, HttpContext.Request.Cookies));
        }

        /// <summary>
        /// Get all user objects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Account.GetAllUsers)]
        public async Task<IActionResult> GetAllUsers()
        {
            return _codeFactory.GetStatusCode(await _service.GetUsers());
        }

        /// <summary>
        /// Get the profile of a single user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route(Route.Account.GetUserProfile)]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            return _codeFactory.GetStatusCode(await _service.GetUserProfile(id));
        }

        /// <summary>
        /// Get all user ids
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route(Route.Account.GetAllOtherUsers)]
        public async Task<IActionResult> GetAllOtherUsers()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllOtherUsers());
        }

        /// <summary>
        /// Fuzzy user search
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route(Route.Account.FuzzyUserSearch)]
        public async Task<IActionResult> GetAllOtherUsers(string query)
        {
            return _codeFactory.GetStatusCode(await _service.FuzzyUserSearch(query));
        }

        /// <summary>
        /// Update the active user's avatar image
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [Route(Route.Account.UpdateAvatar)]
        public async Task<IActionResult> UpdateUserAvatar(UploadRegularFileRequest request)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateUserAvatar(request?.File));
        }

        /// <summary>
        /// Update the active user's avatar image
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [Route(Route.Account.UpdateDisplayName)]
        public async Task<IActionResult> UpdateDisplayName([FromBody] string name)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateDisplayName(name));
        }

        /// <summary>
        /// Gets the list of all IP addresses who have attempted logins
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Account.GetGuestLogins)]
        public async Task<IActionResult> GetGuestLogins()
        {
            return _codeFactory.GetStatusCode(await _service.GetGuestLogins());
        }

        /// <summary>
        /// Unblocks an IP address through the guest login system
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Account.UnblockGuest)]
        public async Task<IActionResult> UnblockGuest([FromBody] string guest)
        {
            return _codeFactory.GetStatusCode(await _service.UnblockGuestIP(guest));
        }
    }
}
