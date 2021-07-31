using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Models;
using VueServer.Models.Account;
using VueServer.Models.Directory;
using VueServer.Services.Interface;
using static VueServer.Domain.DomainConstants.Authentication;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
{
    [Route(Route.Admin.Controller)]
    public class AdminController
    {
        private readonly IAdminService _adminService;
        private readonly IAccountService _accountService;
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        // Initializes all components to be able to interface with the Identity Framework
        // To allow users to log in, register, and logout, register roles, etc.
        public AdminController(
            IAdminService adminService,
            IAccountService accountService,
            IStatusCodeFactory<IActionResult> factory
        )
        {
            _adminService = adminService ?? throw new ArgumentNullException("Admin service is null");
            _accountService = accountService ?? throw new ArgumentNullException("Account service is null");
            _codeFactory = factory ?? throw new ArgumentNullException("Code factory is null");
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.ChangePassword)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            return _codeFactory.GetStatusCode(await _accountService.ChangePassword(model, true));
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.GetAllRoles)]
        public async Task<IActionResult> GetAllRoles()
        {
            return _codeFactory.GetStatusCode(await _accountService.GetRoles());
        }



        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.Directory.GetDirectorySettings)]
        public async Task<IActionResult> GetDirectorySettings()
        {
            return _codeFactory.GetStatusCode(await _adminService.GetDirectorySettings());
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.SetServerSetting)]
        public async Task<IActionResult> SetServerSetting([FromBody] ServerSettings setting)
        {
            return _codeFactory.GetStatusCode(await _adminService.SetServerSetting(setting));
        }

        [HttpDelete]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.DeleteServerSetting)]
        public async Task<IActionResult> SetServerSetting(string key)
        {
            return _codeFactory.GetStatusCode(await _adminService.DeleteServerSetting(key));
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.Directory.GetGroupDirectories)]
        public async Task<IActionResult> GetGroupDirectories()
        {
            return _codeFactory.GetStatusCode(await _adminService.GetGroupDirectories());
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.Directory.GetUserDirectories)]
        public async Task<IActionResult> GetUserDirectories()
        {
            return _codeFactory.GetStatusCode(await _adminService.GetUserDirectories());
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.Directory.AddGroupDirectory)]
        public async Task<IActionResult> AddGroupDirectory([FromBody] ServerGroupDirectory dir)
        {
            return _codeFactory.GetStatusCode(await _adminService.AddGroupDirectory(dir));
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.Directory.AddUserDirectory)]
        public async Task<IActionResult> AddUserDirectory([FromBody] ServerUserDirectory dir)
        {
            return _codeFactory.GetStatusCode(await _adminService.AddUserDirectory(dir));
        }

        [HttpDelete]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.Directory.DeleteGroupDirectory)]
        public async Task<IActionResult> DeleteGroupDirectory(int id)
        {
            return _codeFactory.GetStatusCode(await _adminService.DeleteGroupDirectory(id));
        }

        [HttpDelete]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Admin.Directory.DeleteUserDirectory)]
        public async Task<IActionResult> DeleteUserDirectory(long id)
        {
            return _codeFactory.GetStatusCode(await _adminService.DeleteUserDirectory(id));
        }
    }
}
