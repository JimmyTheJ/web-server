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
using VueServer.Common.Factory.Interface;
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
            _codeFactory = codeFactory;
            _service = service;
        }

        [Authorize(Roles = "Administrator")]
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
