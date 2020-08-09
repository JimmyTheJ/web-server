using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using static VueServer.Domain.Constants.Authentication;
using VueServer.Domain.Factory.Interface;
using VueServer.Services.Interface;
using System.Threading.Tasks;
using VueServer.Models;
using AddOns = VueServer.Domain.Constants.Models.ModuleAddOns;
using Features = VueServer.Domain.Constants.Models.ModuleFeatures;
using VueServer.Controllers.Filters;

namespace VueServer.Controllers
{
    [Route("/api/weight")]
    public class WeightController : Controller
    {
        private readonly IWeightService  _service;

        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public WeightController(
            IWeightService service, 
            IStatusCodeFactory<IActionResult> codeFactory)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Weight service is null");
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route("list")]
        public async Task<IActionResult> GetWeightHistory()
        {
            return _codeFactory.GetStatusCode(await _service.GetWeightList());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route("add")]
        public async Task<IActionResult> AddWeight([FromBody] Weight weight)
        {
            return _codeFactory.GetStatusCode(await _service.AddWeight(weight));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route("edit")]
        public async Task<IActionResult> EditWeight([FromBody] Weight weight)
        {
            return _codeFactory.GetStatusCode(await _service.EditWeight(weight));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Weight.Id)]
        [Route("delete")]
        public async Task<IActionResult> DeleteWeight(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteWeight(id));
        }
    }
}
