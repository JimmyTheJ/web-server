using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.Status;
using VueServer.Models;
using VueServer.Services.Weight;
using static VueServer.Domain.DomainConstants.Authentication;
using AddOns = VueServer.Domain.DomainConstants.Models.ModuleAddOns;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
{
    [Route(Route.Weight.Controller)]
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
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route(Route.Generic.List)]
        public async Task<IActionResult> GetWeightHistory()
        {
            return _codeFactory.GetStatusCode(await _service.GetWeightList());
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route(Route.Generic.Add)]
        public async Task<IActionResult> AddWeight([FromBody] Weights weight)
        {
            return _codeFactory.GetStatusCode(await _service.AddWeight(weight));
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route(Route.Generic.Edit)]
        public async Task<IActionResult> EditWeight([FromBody] Weights weight)
        {
            return _codeFactory.GetStatusCode(await _service.EditWeight(weight));
        }

        [HttpDelete]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route(Route.Generic.Delete)]
        public async Task<IActionResult> DeleteWeight(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteWeight(id));
        }
    }
}
