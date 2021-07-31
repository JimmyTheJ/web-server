using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Models.Modules;
using VueServer.Services.Interface;
using static VueServer.Domain.DomainConstants.Authentication;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
{
    [Route(Route.Module.Controller)]
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
        [Route(Route.Module.GetModulesForUser)]
        public async Task<IActionResult> GetActiveModules()
        {
            return _codeFactory.GetStatusCode(await _service.GetActiveModulesForUser());
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Module.GetUserModuleAndFeatures)]
        public async Task<IActionResult> GetAllModulesAndFeatures(string user)
        {
            return _codeFactory.GetStatusCode(await _service.GetModulesAndFeaturesForOtherUser(user));
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Module.GetAllModules)]
        public async Task<IActionResult> GetAllModules()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllModules());
        }

        [HttpGet]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Module.GetModulesForAllUsers)]
        public async Task<IActionResult> GetModulesForAllUsers()
        {
            return _codeFactory.GetStatusCode(await _service.GetActiveModulesForAllUsers());
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Module.AddModuleToUser)]
        public async Task<IActionResult> AddModuleToUser([FromBody] UserHasModuleAddOn module)
        {
            return _codeFactory.GetStatusCode(await _service.AddModuleToUser(module));
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Module.DeleteModuleFromUser)]
        public async Task<IActionResult> DeleteModuleFromUser([FromBody] UserHasModuleAddOn module)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteModuleFromUser(module));
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Module.AddFeatureToUser)]
        public async Task<IActionResult> AddFeatureToUser([FromBody] UserHasModuleFeature feature)
        {
            return _codeFactory.GetStatusCode(await _service.AddFeatureToUser(feature));
        }

        [HttpPost]
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route(Route.Module.DeleteFeatureFromUser)]
        public async Task<IActionResult> DeleteFeatureFromUser([FromBody] UserHasModuleFeature feature)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteFeatureFromUser(feature));
        }
    }
}
