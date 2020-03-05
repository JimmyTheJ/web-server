using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Factory.Interface;
using VueServer.Models.Models.Request;
using VueServer.Services.Interface;
using static VueServer.Domain.Constants;

namespace VueServer.Controllers.Controllers
{
    [Route("api/library")]
    public class LibraryController : Controller
    {
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        private readonly ILibraryService _service;

        public LibraryController(
            IStatusCodeFactory<IActionResult> codeFactory,
            ILibraryService service)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Library service is null");
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("book/list")]
        public async Task<IActionResult> GetBookList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllBooks());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [Route("book/add")]
        public async Task<IActionResult> AddBook([FromBody] BookAddRequest request)
        {
            return _codeFactory.GetStatusCode(await _service.CreateBook(request));
        }

        [HttpPut]
        [Authorize(Roles = ROLES_ALL)]
        [Route("book/update")]
        public async Task<IActionResult> UpdateBook([FromBody] BookAddRequest request)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateBook(request));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [Route("book/delete")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteBook(id));
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("genre/list")]
        public async Task<IActionResult> GetGenreList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllGenres());
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("author/list")]
        public async Task<IActionResult> GetAuthorList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllAuthors());
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("series/list")]
        public async Task<IActionResult> GetSeriesList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllSeries());
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("bookshelf/list")]
        public async Task<IActionResult> GetBookshelfList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllBookshelves());
        }
    }
}
