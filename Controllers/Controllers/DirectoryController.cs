using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static VueServer.Domain.Constants.Authentication;
using VueServer.Domain.Enums;
using VueServer.Domain.Factory.Interface;
using VueServer.Models;
using VueServer.Models.Request;
using VueServer.Services.Interface;

namespace VueServer.Controllers
{
    [Route("api/directory")]
    public class DirectoryController : Controller
    {
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;
        
        private readonly IDirectoryService _service;

        public DirectoryController (
            IStatusCodeFactory<IActionResult> codeFactory,
            IDirectoryService service)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Directory service is null");
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = ROLES_ALL)]
        [Route("download/file/{*filename}")]
        public async Task<IActionResult> DownloadProtectedFile(string filename)
        {
            var file = await _service.Download(filename);
            if (file == null || file.Obj == null)
                return BadRequest();
            else
                return PhysicalFile(file.Obj.Item1, file.Obj.Item2, file.Obj.Item3);
        }
        
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = ROLES_ALL)]
        [Route("/api/serve-file/{*filename}")]
        public async Task<IActionResult> ServeMedia(string filename)
        {
            var file = await _service.Download(filename, true);
            if (file.Code != Domain.Enums.StatusCode.OK && file.Code != Domain.Enums.StatusCode.NO_CONTENT)
            {
                return _codeFactory.GetStatusCode(file);
            }
            else
            {
                var rangeFile = PhysicalFile(file.Obj.Item1, file.Obj.Item2, file.Obj.Item3);
                rangeFile.EnableRangeProcessing = true;
                return rangeFile;
            }
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("folder/{directory}/{*dir}")]
        public IActionResult LoadDirectory(string directory, string dir = null)
        {
            return _codeFactory.GetStatusCode(_service.Load(directory, dir));
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("list")]
        public IActionResult GetDirectoryList()
        {
            return _codeFactory.GetStatusCode(_service.GetDirectories());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [Route("delete")]
        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Administrator")]
        public IActionResult Delete([FromBody] DeleteFileModel model)
        {
            return _codeFactory.GetStatusCode(_service.Delete(model));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [Route("upload")]
        public async Task<IActionResult> UploadAsync([FromForm] UploadFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Upload(model));
        }
    }    
}
