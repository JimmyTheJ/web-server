using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using VueServer.Common.Factory.Interface;
using VueServer.Models;
using VueServer.Services.Interface;

namespace VueServer.Controllers
{
    [Authorize]
    [Route("api/upload")]
    public class UploadController : Controller
    {
        private readonly IUploadService  _service;

        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public UploadController(
            IUploadService service, 
            IStatusCodeFactory<IActionResult> codeFactory)
        {
            _codeFactory = codeFactory;
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Elevated")]
        [Route("folders")]
        public IActionResult GetFolderList()
        {
            return _codeFactory.GetStatusCode(_service.GetFolderList());
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Elevated")]
        [Route("list")]
        public IActionResult List()
        {
            return _codeFactory.GetStatusCode(_service.GetFiles());
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [Route("delete")]
        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Administrator")]
        public IActionResult Delete([FromBody] DeleteFileModel model)
        {
            return _codeFactory.GetStatusCode(_service.Delete(model));
        }

        /// <summary>
        /// TODO: FILL THIS IN
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, Elevated")]
        [Route("upload")]
        public async Task<IActionResult> UploadAsync (UploadFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Upload(model));
        }
    }
}
