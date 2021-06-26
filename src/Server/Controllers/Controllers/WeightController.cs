using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.StatusFactory;
using VueServer.Models;
using VueServer.Services.Interface;
using static VueServer.Domain.Constants.Authentication;
using AddOns = VueServer.Domain.Constants.Models.ModuleAddOns;

namespace VueServer.Controllers
{
    [Route("/api/weight")]
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
        [Route("list")]
        public async Task<IActionResult> GetWeightHistory()
        {
            return _codeFactory.GetStatusCode(await _service.GetWeightList());
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route("add")]
        public async Task<IActionResult> AddWeight([FromBody] Weight weight)
        {
            return _codeFactory.GetStatusCode(await _service.AddWeight(weight));
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route("edit")]
        public async Task<IActionResult> EditWeight([FromBody] Weight weight)
        {
            return _codeFactory.GetStatusCode(await _service.EditWeight(weight));
        }

        [HttpDelete]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route("delete")]
        public async Task<IActionResult> DeleteWeight(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteWeight(id));
        }
    }
}
