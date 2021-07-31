using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.Status;
using VueServer.Models.Library;
using VueServer.Models.Request;
using VueServer.Services.Interface;
using static VueServer.Domain.DomainConstants.Authentication;
using AddOns = VueServer.Domain.DomainConstants.Models.ModuleAddOns;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
{
    [Route(Route.Library.Controller)]
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

        #region -> Book

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Book + "/" + Route.Generic.List)]
        public async Task<IActionResult> GetBookList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllBooks());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Book + "/" + Route.Generic.Add)]
        public async Task<IActionResult> AddBook([FromBody] BookAddRequest request)
        {
            return _codeFactory.GetStatusCode(await _service.CreateBook(request));
        }

        [HttpPut]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Book + "/" + Route.Generic.Update)]
        public async Task<IActionResult> UpdateBook([FromBody] BookAddRequest request)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateBook(request));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Book + "/" + Route.Generic.Delete)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteBook(id));
        }

        #endregion

        #region -> Author

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Author + "/" + Route.Generic.List)]
        public async Task<IActionResult> GetAuthorList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllAuthors());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Author + "/" + Route.Generic.Add)]
        public async Task<IActionResult> AddAuthor([FromBody] Author request)
        {
            return _codeFactory.GetStatusCode(await _service.CreateAuthor(request));
        }

        [HttpPut]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Author + "/" + Route.Generic.Update)]
        public async Task<IActionResult> UpdateAuthor([FromBody] Author request)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateAuthor(request));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Author + "/" + Route.Generic.Delete)]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteAuthor(id));
        }

        #endregion

        #region -> Bookcase

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Bookcase + "/" + Route.Generic.List)]
        public async Task<IActionResult> GetBookcaseList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllBookcases());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Bookcase + "/" + Route.Generic.Add)]
        public async Task<IActionResult> AddBookcase([FromBody] Bookcase request)
        {
            return _codeFactory.GetStatusCode(await _service.CreateBookcase(request));
        }

        [HttpPut]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Bookcase + "/" + Route.Generic.Update)]
        public async Task<IActionResult> UpdateBookcase([FromBody] Bookcase request)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateBookcase(request));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Bookcase + "/" + Route.Generic.Delete)]
        public async Task<IActionResult> DeleteBookcase(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteBookcase(id));
        }

        #endregion

        #region -> Genre

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Genre + "/" + Route.Generic.List)]
        public async Task<IActionResult> GetGenreList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllGenres());
        }

        #endregion

        #region -> Series

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Series + "/" + Route.Generic.List)]
        public async Task<IActionResult> GetSeriesList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllSeries());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Series + "/" + Route.Generic.Add)]
        public async Task<IActionResult> AddSeries([FromBody] Series request)
        {
            return _codeFactory.GetStatusCode(await _service.CreateSeries(request));
        }

        [HttpPut]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Series + "/" + Route.Generic.Update)]
        public async Task<IActionResult> UpdateSeries([FromBody] Series request)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateSeries(request));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Series + "/" + Route.Generic.Delete)]
        public async Task<IActionResult> DeleteSeries(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteSeries(id));
        }

        #endregion

        #region -> Shelf

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Shelf + "/" + Route.Generic.List)]
        public async Task<IActionResult> GetShelfList()
        {
            return _codeFactory.GetStatusCode(await _service.GetAllShelves());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Shelf + "/" + Route.Generic.Add)]
        public async Task<IActionResult> AddShelf([FromBody] Shelf request)
        {
            return _codeFactory.GetStatusCode(await _service.CreateShelf(request));
        }

        [HttpPut]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Shelf + "/" + Route.Generic.Update)]
        public async Task<IActionResult> UpdateShelf([FromBody] Shelf request)
        {
            return _codeFactory.GetStatusCode(await _service.UpdateShelf(request));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Library.Id)]
        [Route(Route.Library.Shelf + "/" + Route.Generic.Delete)]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteShelf(id));
        }

        #endregion
    }
}
