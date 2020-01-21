using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static VueServer.Domain.Constants;
using VueServer.Domain.Enums;
using VueServer.Domain.Factory.Interface;
using VueServer.Models;
using VueServer.Services.Interface;
using VueServer.Controllers.Helpers;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;

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
            var range = GetRange(HttpContext.Request.Headers);

            var file = await _service.StreamMedia(filename, range.Item1, range.Item2);
            //var file = await _service.StreamMedia(filename, 0l, 300000000l);
            if (file == null || file.Obj == null)
                return BadRequest();
            else
            {
                HttpContext.Response.Headers.Add("accept-ranges", "bytes");
                HttpContext.Response.Headers.Add("content-length", file.Obj.Item3.ToString());
                HttpContext.Response.Headers.Add("content-range", $"bytes {range.Item1}-{(file.Obj.Item3-1).ToString()}");
                return PhysicalFile(file.Obj.Item1, file.Obj.Item2, false);

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
        [Authorize(Roles = ADMINISTRATOR_STRING)]
        [Route("delete")]
        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Administrator")]
        public IActionResult Delete([FromBody] DeleteFileModel model)
        {
            return _codeFactory.GetStatusCode(_service.Delete(model));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ADMIN_ELEVATED)]
        [Route("upload")]
        public async Task<IActionResult> UploadAsync([FromForm] UploadFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Upload(model));
        }

        private Tuple<long, long> GetRange (IHeaderDictionary headers)
        {
            try
            {
                if (!headers.TryGetValue("Range", out var rangeValues))
                {
                    // No range attribute
                    return new Tuple<long, long>(0, 0);
                }

                var range = rangeValues.First();
                if (range.StartsWith("bytes="))
                {
                    var nums = range.Substring("bytes=".Length);

                    var split = nums.Split('-');
                    if (split == null || split.Length != 2)
                    {
                        // If somehow this doesn't work
                        return new Tuple<long, long>(0, 0);
                    }

                    long start = Convert.ToInt64(split[0]);
                    long end = 0;
                    if (split[1] == "")
                    {
                        end = -1;
                    }
                    else
                    {
                        end = Convert.ToInt64(split[1]);
                    }

                    Console.WriteLine($"GetRange: Start: {start}\t End: {end}");
                    return new Tuple<long, long>(start, end);
                }
                else
                {
                    return new Tuple<long, long>(0, 0);
                }
            }
            catch
            {
                return new Tuple<long, long>(0, 0);
            }
            
        }
    }    
}
