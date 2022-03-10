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
    [Route(Route.Admin.Controller)]
    [Authorize(DomainConstants.Authentication.ADMINISTRATOR_STRING)]
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
        [Route(Route.Admin.Directory.GetDirectorySettings)]
        public async Task<IActionResult> GetDirectorySettings()
        {
            return _codeFactory.GetStatusCode(await _service.GetDirectorySettings());
        }

        [HttpPost]
        [Route(Route.Admin.Directory.AddGroupDirectory)]
        public async Task<IActionResult> AddGroupDirectory([FromBody] ServerGroupDirectory dir)
        {
            return _codeFactory.GetStatusCode(await _service.AddGroupDirectory(dir));
        }

        [HttpPost]
        [Route(Route.Admin.Directory.AddUserDirectory)]
        public async Task<IActionResult> AddUserDirectory([FromBody] ServerUserDirectory dir)
        {
            return _codeFactory.GetStatusCode(await _service.AddUserDirectory(dir));
        }

        [HttpGet]
        [Route(Route.Admin.Directory.GetGroupDirectories)]
        public async Task<IActionResult> GetGroupDirectories()
        {
            return _codeFactory.GetStatusCode(await _service.GetGroupDirectories());
        }

        [HttpGet]
        [Route(Route.Admin.Directory.GetUserDirectories)]
        public async Task<IActionResult> GetUserDirectories()
        {
            return _codeFactory.GetStatusCode(await _service.GetUserDirectories());
        }

        [HttpDelete]
        [Route(Route.Admin.Directory.DeleteGroupDirectory)]
        public async Task<IActionResult> DeleteGroupDirectory(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteGroupDirectory(id));
        }

        [HttpDelete]
        [Route(Route.Admin.Directory.DeleteUserDirectory)]
        public async Task<IActionResult> DeleteUserDirectory(long id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteUserDirectory(id));
        }

        [HttpPost]
        [Route(Route.Admin.Directory.CreateDefaultFolder)]
        public async Task<IActionResult> CreateDefaultFolder(string username)
        {
            return _codeFactory.GetStatusCode(await _service.CreateDefaultFolder(username));
        }
    }
}
