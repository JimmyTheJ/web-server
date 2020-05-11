using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Factory.Interface;
using VueServer.Models.Modules;
using VueServer.Services.Interface;

using static VueServer.Domain.Constants;

namespace VueServer.Controllers
{
    [Route("api/modules")]
    public class ModuleController : Controller
    {
        private readonly IModuleService _service;

        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public ModuleController(
            IModuleService service,
            IStatusCodeFactory<IActionResult> factory
        )
        {
            _service = service ?? throw new ArgumentNullException("Module service is null");
            _codeFactory = factory ?? throw new ArgumentNullException("Code factory is null");
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("get-modules-for-user")]
        public async Task<IActionResult> GetActiveModules()
        {
            return _codeFactory.GetStatusCode(await _service.GetActiveModulesForUser());
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route("get-all-modules")]
        public async Task<IActionResult> GetAllModules()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllModules());
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route("get-modules-for-all-users")]
        public async Task<IActionResult> GetModulesForAllUsers()
        {
            return _codeFactory.GetStatusCode(await _service.GetActiveModulesForAllUsers());
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route("add-module-to-user")]
        public async Task<IActionResult> AddModuleToUser([FromBody] UserHasModuleAddOn module)
        {
            return _codeFactory.GetStatusCode(await _service.AddModuleToUser(module));
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route("delete-module-from-user")]
        public async Task<IActionResult> DeleteModuleFromUser([FromBody] UserHasModuleAddOn module)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteModuleFromUser(module));
        }
    }
}
