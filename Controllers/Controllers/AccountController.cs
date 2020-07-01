using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using VueServer.Models.Account;
using VueServer.Services.Interface;
using VueServer.Domain.Factory.Interface;

using static VueServer.Domain.Constants.Authentication;
using VueServer.Models.Modules;
using VueServer.Models.Request;

namespace VueServer.Controllers
{
    [Route("/api/account")]
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
        [IgnoreAntiforgeryToken]
        [Route("register")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Register(model));
        }

        // Login to a existing user account using the Identity Framework
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Login(model));
        }
        
        /// <summary>
        /// Logout of your account
        /// Authorize checks and sees if the user is logged in to the Identity Framework
        /// All of the null checks and other stuff is done already for you using this attribute
        /// </summary>
        /// <returns></returns>
        [HttpPost, HttpGet]
        [Authorize]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            return _codeFactory.GetStatusCode(await _service.Logout(HttpContext));
        }

        /// <summary>
        /// Get a new CSRF token on a page refresh most likely
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [IgnoreAntiforgeryToken]
        [Route("get-csrf-token")]
        public IActionResult GetCSRFToken ()
        {
            return _codeFactory.GetStatusCode(_service.GetCsrfToken(HttpContext));
        }

        /// <summary>
        /// Refresh the user's JWT token
        /// </summary>
        /// <param name="accessToken">Old token to be refreshed</param>
        /// <returns></returns>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("refresh-jwt")]
        public async Task<IActionResult> RefreshToken ([FromBody] RefreshTokenRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.RefreshJwtToken(model));
        }

        /// <summary>
        /// Get all user objects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route("user/get-all")]
        public async Task<IActionResult> GetAllUsers ()
        {
            return _codeFactory.GetStatusCode(await _service.GetUsers());
        }

        /// <summary>
        /// Get all user ids
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("user/get-all-ids")]
        public async Task<IActionResult> GetAllUserIds()
        {
            return _codeFactory.GetStatusCode(await _service.GetUserIds());
        }

        /// <summary>
        /// Update the active user's avatar image
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [Route("user/update-avatar")]
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
        [Route("user/update-displayname")]
        public async Task<IActionResult> UpdateDisplayName(string name)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateDisplayName(name));
        }

        #endregion
    }
}
