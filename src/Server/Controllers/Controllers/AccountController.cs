using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Models.Account;
using VueServer.Models.Request;
using VueServer.Services.Interface;
using static VueServer.Domain.DomainConstants.Authentication;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
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

        #region -> Actions

        /// <summary>
        /// Registering a new user into the Identity Framework
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Route.Account.Register)]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Register(model));
        }

        // Login to a existing user account using the Identity Framework
        [HttpPost]
        [Route(Route.Account.Login)]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Login(HttpContext, model));
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
            return _codeFactory.GetStatusCode(await _service.Logout(HttpContext, username));
        }

        /// <summary>
        /// Refresh the user's JWT token
        /// </summary>
        /// <param name="accessToken">Old token to be refreshed</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Route.Account.RefreshJwt)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.RefreshJwtToken(model));
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

        #endregion
    }
}
