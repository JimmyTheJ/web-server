using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

using VueServer.Common.Factory.Interface;
using VueServer.Models;
using VueServer.Services.Interface;

namespace VueServer.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/directory")]
    public class DirectoryController : Controller
    {
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;
        
        private readonly IDirectoryService _service;

        public DirectoryController (
            IStatusCodeFactory<IActionResult> codeFactory,
            IDirectoryService service)
        {
            _codeFactory = codeFactory;
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Elevated, User")]
        [Route("/download/file/{level:int}/{*filename}")]
        public IActionResult DownloadProtectedFile(int level, string filename)
        {
            var file = _service.Download(level, filename);
            if (file == null || file.Obj == null)
                return BadRequest();
            else
                return PhysicalFile(file.Obj.Item1, file.Obj.Item2, file.Obj.Item3);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Elevated, User")]
        [Route("/load/folder/{level:int}/{directory}/{dir?}")]
        public IActionResult LoadDirectory(string directory, string dir = null, int level = 0)
        {
            return _codeFactory.GetStatusCode(_service.Load(directory, dir, level));
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Elevated, User")]
        [Route("list")]
        public IActionResult GetDirectoryList(int level = Common.Constants.NO_LEVEL)
        {
            return _codeFactory.GetStatusCode(_service.GetDirectories(level));
        }
    }
}
