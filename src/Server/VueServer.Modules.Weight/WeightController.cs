using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Modules.Core.Controllers.Filters;
using VueServer.Modules.Weight.Models;
using VueServer.Modules.Weight.Services;
using static VueServer.Domain.DomainConstants.Authentication;
using Route = VueServer.Modules.Core.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Modules.Weight
{
    [Route(WeightConstants.Controller.BasePath)]
    [Authorize(Roles = ROLES_ALL)]
    public class WeightController : Controller
    {
        private readonly IWeightService _service;

        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public WeightController(
            IWeightService service,
            IStatusCodeFactory<IActionResult> codeFactory)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Weight service is null");
        }

        [HttpGet]
        [ModuleAuthFilterFactory(Module = WeightConstants.ModuleAddOn.Id)]
        [Route(Route.Generic.List)]
        public async Task<IActionResult> GetWeightHistory()
        {
            return _codeFactory.GetStatusCode(await _service.GetWeightList());
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = WeightConstants.ModuleAddOn.Id)]
        [Route(Route.Generic.Add)]
        public async Task<IActionResult> AddWeight([FromBody] Weights weight)
        {
            return _codeFactory.GetStatusCode(await _service.AddWeight(weight));
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = WeightConstants.ModuleAddOn.Id)]
        [Route(Route.Generic.Edit)]
        public async Task<IActionResult> EditWeight([FromBody] Weights weight)
        {
            return _codeFactory.GetStatusCode(await _service.EditWeight(weight));
        }

        [HttpDelete]
        [ModuleAuthFilterFactory(Module = WeightConstants.ModuleAddOn.Id)]
        [Route(Route.Generic.Delete)]
        public async Task<IActionResult> DeleteWeight(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteWeight(id));
        }
    }
}
