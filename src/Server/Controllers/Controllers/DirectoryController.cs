using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.StatusFactory;
using VueServer.Domain;
using VueServer.Models;
using VueServer.Models.Request;
using VueServer.Services.Interface;
using static VueServer.Domain.Constants.Authentication;
using AddOns = VueServer.Domain.Constants.Models.ModuleAddOns;
using Features = VueServer.Domain.Constants.Models.ModuleFeatures;

namespace VueServer.Controllers
{
    [Route("api/directory")]
    public class DirectoryController : Controller
    {
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;
        private readonly IDirectoryService _service;
        private readonly IAccountService _accountService;
        private readonly IModuleService _moduleService;

        public DirectoryController(
            IStatusCodeFactory<IActionResult> codeFactory,
            IDirectoryService service,
            IAccountService accountService,
            IModuleService moduleService)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Directory service is null");
            _accountService = accountService ?? throw new ArgumentNullException("Account service is null");
            _moduleService = moduleService ?? throw new ArgumentNullException("Module service is null");
        }

        [HttpGet]
        [Route("download/file/{*filename}")]
        public async Task<IActionResult> DownloadProtectedFile(string filename, string token)
        {
            var validated = _accountService.ValidateTokenAndGetName(token);
            if (validated.Code != Domain.Enums.StatusCode.OK)
            {
                return Unauthorized();
            }

            var userHasModule = await _moduleService.DoesUserHaveModule(validated.Obj, Constants.Models.ModuleAddOns.Browser.Id);
            if (userHasModule.Obj == false)
            {
                return Unauthorized();
            }

            var file = await _service.Download(filename, validated.Obj);
            if (file == null || file.Obj == null)
                return BadRequest();
            else
                return PhysicalFile(file.Obj.Item1, file.Obj.Item2, file.Obj.Item3);
        }

        [HttpGet]
        [Route("/api/serve-file/{*filename}")]
        public async Task<IActionResult> ServeMedia(string filename, string token)
        {
            var validated = _accountService.ValidateTokenAndGetName(token);
            if (validated.Code != Domain.Enums.StatusCode.OK)
            {
                return Unauthorized();
            }

            var userHasModule = await _moduleService.DoesUserHaveModule(validated.Obj, Constants.Models.ModuleAddOns.Browser.Id);
            if (userHasModule.Obj == false)
            {
                return Unauthorized();
            }

            var userHasFeature = await _moduleService.DoesUserHaveFeature(validated.Obj, Constants.Models.ModuleFeatures.Browser.VIEWER_ID);
            if (userHasFeature.Obj == false)
            {
                return Unauthorized();
            }

            var file = await _service.Download(filename, validated.Obj, true);
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
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id)]
        [Route("folder/{directory}/{*dir}")]
        public async Task<IActionResult> LoadDirectory(string directory, string dir = null)
        {
            return _codeFactory.GetStatusCode(await _service.Load(directory, dir));
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id)]
        [Route("list")]
        public async Task<IActionResult> GetDirectoryList()
        {
            return _codeFactory.GetStatusCode(await _service.GetDirectories());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.DELETE_ID)]
        [Route("delete")]
        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Administrator")]
        public async Task<IActionResult> Delete([FromBody] DeleteFileModel model)
        {
            return _codeFactory.GetStatusCode(await _service.Delete(model));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.UPLOAD_ID)]
        [Route("upload")]
        public async Task<IActionResult> UploadAsync([FromForm] UploadDirectoryFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Upload(model));
        }
    }
}
