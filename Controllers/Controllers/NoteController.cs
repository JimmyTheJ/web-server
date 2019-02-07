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
using static VueServer.Domain.Constants;
using VueServer.Domain.Factory.Interface;
using VueServer.Models;
using VueServer.Models.Account;
using VueServer.Models.Context;
using VueServer.Services.Interface;

namespace VueServer.Controllers
{
    [Authorize]
    [Route("api/note")]
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
        [Route("getall")]
        public async Task<IActionResult> GetAll()
        {
            return _codeFactory.GetStatusCode(await _service.GetAll());
        }

        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> Get()
        {
            var thing = HttpContext.User;
            return _codeFactory.GetStatusCode(await _service.Get());
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] Notes note)
        {
            return _codeFactory.GetStatusCode(await _service.Create(note));
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] Notes note)
        {
            return _codeFactory.GetStatusCode(await _service.Update(note));
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete (int id)
        {
            return _codeFactory.GetStatusCode(await _service.Delete(id));
        }
    }
}
