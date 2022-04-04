using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Domain;
using VueServer.Modules.Directory.Models;
using VueServer.Modules.Directory.Services;
using Route = VueServer.Modules.Core.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Modules.Directory.Controllers
{
    [Authorize(Roles = DomainConstants.Authentication.ADMINISTRATOR_STRING)]
    [Route(DirectoryConstants.Controller.BasePath)]
    public class DirectoryAdminController : Controller
    {
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;
        private readonly IDirectoryAdminService _service;

        public DirectoryAdminController(IStatusCodeFactory<IActionResult> codeFactory, IDirectoryAdminService service)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Directory service is null");
        }

        [HttpGet]
        [Route(DirectoryConstants.Controller.Admin.GetDirectorySettings)]
        public async Task<IActionResult> GetDirectorySettings()
        {
            return _codeFactory.GetStatusCode(await _service.GetDirectorySettings());
        }

        [HttpPost]
        [Route(DirectoryConstants.Controller.Admin.AddGroupDirectory)]
        public async Task<IActionResult> AddGroupDirectory([FromBody] ServerGroupDirectory dir)
        {
            return _codeFactory.GetStatusCode(await _service.AddGroupDirectory(dir));
        }

        [HttpPost]
        [Route(DirectoryConstants.Controller.Admin.AddUserDirectory)]
        public async Task<IActionResult> AddUserDirectory([FromBody] ServerUserDirectory dir)
        {
            return _codeFactory.GetStatusCode(await _service.AddUserDirectory(dir));
        }

        [HttpGet]
        [Route(DirectoryConstants.Controller.Admin.GetGroupDirectories)]
        public async Task<IActionResult> GetGroupDirectories()
        {
            return _codeFactory.GetStatusCode(await _service.GetGroupDirectories());
        }

        [HttpGet]
        [Route(DirectoryConstants.Controller.Admin.GetUserDirectories)]
        public async Task<IActionResult> GetUserDirectories()
        {
            return _codeFactory.GetStatusCode(await _service.GetUserDirectories());
        }

        [HttpDelete]
        [Route(DirectoryConstants.Controller.Admin.DeleteGroupDirectory)]
        public async Task<IActionResult> DeleteGroupDirectory(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteGroupDirectory(id));
        }

        [HttpDelete]
        [Route(DirectoryConstants.Controller.Admin.DeleteUserDirectory)]
        public async Task<IActionResult> DeleteUserDirectory(long id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteUserDirectory(id));
        }

        [HttpPost]
        [Route(DirectoryConstants.Controller.Admin.CreateDefaultFolder)]
        public async Task<IActionResult> CreateDefaultFolder(string username)
        {
            return _codeFactory.GetStatusCode(await _service.CreateDefaultFolder(username));
        }
    }
}
