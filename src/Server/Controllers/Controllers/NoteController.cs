using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.Status;
using VueServer.Models;
using VueServer.Services.Interface;
using static VueServer.Domain.DomainConstants.Authentication;
using AddOns = VueServer.Domain.DomainConstants.Models.ModuleAddOns;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
{
    [Route(Route.Note.Controller)]
    [Authorize]
    public class NoteController : Controller
    {
        private readonly INoteService _service;

        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public NoteController(
            INoteService service,
            IStatusCodeFactory<IActionResult> codeFactory)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Note service is null");
        }

        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [HttpGet]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route(Route.Generic.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return _codeFactory.GetStatusCode(await _service.GetAll());
        }

        [HttpGet]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route(Route.Generic.Get)]
        public async Task<IActionResult> Get()
        {
            return _codeFactory.GetStatusCode(await _service.Get());
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route(Route.Generic.Create)]
        public async Task<IActionResult> Create([FromBody] Notes note)
        {
            return _codeFactory.GetStatusCode(await _service.Create(note));
        }

        [HttpPut]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route(Route.Generic.Update)]
        public async Task<IActionResult> Update([FromBody] Notes note)
        {
            return _codeFactory.GetStatusCode(await _service.Update(note));
        }

        [HttpDelete]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route(Route.Generic.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            return _codeFactory.GetStatusCode(await _service.Delete(id));
        }
    }
}
