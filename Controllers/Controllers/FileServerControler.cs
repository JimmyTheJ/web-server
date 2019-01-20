using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using VueServer.Common.Factory.Interface;
using VueServer.Services.Interface;

namespace VueServer.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/file-server")]
    public class FileServerControler : Controller
    {
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;
        
        private readonly IFileServerService _service;

        private readonly IHostingEnvironment _env;

        public FileServerControler(IHostingEnvironment env,
            IStatusCodeFactory<IActionResult> codeFactory,
            IFileServerService service)
        {
            _env = env;
            _codeFactory = codeFactory;
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Elevated, User")]
        [Route("list")]
        public IActionResult GetFileServerFiles()
        {
            return _codeFactory.GetStatusCode(_service.GetFiles(_env.WebRootPath));
        }

        //[HttpGet]
        //[Authorize(Roles = "Administrator, Elevated, User")]
        //public IActionResult DownloadFile()
        //{
        //    return _codeFactory.GetStatusCode(_service.GetFiles(_env.WebRootPath));
        //}
    }
}
