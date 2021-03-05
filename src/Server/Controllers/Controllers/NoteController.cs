using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static VueServer.Domain.Constants.Authentication;
using VueServer.Domain.Factory.Interface;
using VueServer.Models;
using VueServer.Models.Account;
using VueServer.Models.Context;
using VueServer.Services.Interface;
using AddOns = VueServer.Domain.Constants.Models.ModuleAddOns;
using Features = VueServer.Domain.Constants.Models.ModuleFeatures;
using VueServer.Controllers.Filters;

namespace VueServer.Controllers
{
    [Route("api/note")]
    [Authorize]
    public class NoteController : Controller
    {
        private readonly INoteService  _service;

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
        [Route("getall")]
        public async Task<IActionResult> GetAll()
        {
            return _codeFactory.GetStatusCode(await _service.GetAll());
        }

        [HttpGet]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route("get")]
        public async Task<IActionResult> Get()
        {
            return _codeFactory.GetStatusCode(await _service.Get());
        }

        [HttpPost]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] Notes note)
        {
            return _codeFactory.GetStatusCode(await _service.Create(note));
        }

        [HttpPut]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] Notes note)
        {
            return _codeFactory.GetStatusCode(await _service.Update(note));
        }

        [HttpDelete]
        [ModuleAuthFilterFactory(Module = AddOns.Notes.Id)]
        [Route("delete")]
        public async Task<IActionResult> Delete (int id)
        {
            return _codeFactory.GetStatusCode(await _service.Delete(id));
        }
    }
}
