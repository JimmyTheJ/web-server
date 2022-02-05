using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Controllers.Filters;
using VueServer.Core.Status;
using VueServer.Models;
using VueServer.Models.Request;
using VueServer.Services.Account;
using VueServer.Services.DirectoryBrowser;
using VueServer.Services.Module;
using static VueServer.Domain.DomainConstants.Authentication;
using AddOns = VueServer.Domain.DomainConstants.Models.ModuleAddOns;
using DomainModules = VueServer.Domain.DomainConstants.Models;
using Features = VueServer.Domain.DomainConstants.Models.ModuleFeatures;
using Route = VueServer.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Controllers
{
    [Route(Route.Directory.Controller)]
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
        [Route(Route.Directory.DownloadProtectedFile)]
        public async Task<IActionResult> DownloadProtectedFile(string filename, string token)
        {
            var validated = _accountService.ValidateTokenAndGetName(token);
            if (validated.Code != Domain.Enums.StatusCode.OK)
            {
                return Unauthorized();
            }

            var userHasModule = await _moduleService.DoesUserHaveModule(validated.Obj, DomainModules.ModuleAddOns.Browser.Id);
            if (userHasModule.Obj == false)
            {
                return Unauthorized();
            }

            var file = await _service.Download(filename, validated.Obj);
            if (file == null || file.Obj == null)
            {
                return BadRequest();
            }
            else
            {
                return PhysicalFile(file.Obj.Item1, file.Obj.Item2, file.Obj.Item3);
            }
        }

        [HttpGet]
        [Route(Route.Directory.ServeMedia)]
        public async Task<IActionResult> ServeMedia(string filename, string token)
        {
            var validated = _accountService.ValidateTokenAndGetName(token);
            if (validated.Code != Domain.Enums.StatusCode.OK)
            {
                return Unauthorized();
            }

            var userHasModule = await _moduleService.DoesUserHaveModule(validated.Obj, DomainModules.ModuleAddOns.Browser.Id);
            if (userHasModule.Obj == false)
            {
                return Unauthorized();
            }

            var userHasFeature = await _moduleService.DoesUserHaveFeature(validated.Obj, DomainModules.ModuleFeatures.Browser.VIEWER_ID);
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
        [Route(Route.Directory.LoadDirectory)]
        public async Task<IActionResult> LoadDirectory(string directory, string dir = null)
        {
            return _codeFactory.GetStatusCode(await _service.Load(directory, dir));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.CREATE_ID)]
        [Route(Route.Directory.CreateFolder)]
        public async Task<IActionResult> CreateFolder([FromBody] FileModel model)
        {
            return _codeFactory.GetStatusCode(await _service.CreateFolder(model.Directory, model.SubDirectory, model.Name));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.MOVE_ID)]
        [Route(Route.Directory.CopyFile)]
        public async Task<IActionResult> CopyFile([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.CopyFile(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.MOVE_ID)]
        [Route(Route.Directory.CopyFolder)]
        public async Task<IActionResult> CopyFolder([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.CopyFolder(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.MOVE_ID)]
        [Route(Route.Directory.MoveFile)]
        public async Task<IActionResult> MoveFile([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.MoveFile(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.MOVE_ID)]
        [Route(Route.Directory.MoveFolder)]
        public async Task<IActionResult> MoveFolder([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.MoveFolder(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.MOVE_ID)]
        [Route(Route.Directory.RenameFile)]
        public async Task<IActionResult> RenameFile([FromBody] MoveFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.RenameFile(model));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.MOVE_ID)]
        [Route(Route.Directory.RenameFolder)]
        public async Task<IActionResult> RenameFolder([FromBody] MoveFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.RenameFolder(model));
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id)]
        [Route(Route.Generic.List)]
        public async Task<IActionResult> GetDirectoryList()
        {
            return _codeFactory.GetStatusCode(await _service.GetDirectories());
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.DELETE_ID)]
        [Route(Route.Generic.Delete)]
        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Administrator")]
        public async Task<IActionResult> Delete([FromBody] FileModel model)
        {
            return _codeFactory.GetStatusCode(await _service.Delete(model));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 3000000000)]
        [ModuleAuthFilterFactory(Module = AddOns.Browser.Id, Feature = Features.Browser.UPLOAD_ID)]
        [Route(Route.Directory.Upload)]
        public async Task<IActionResult> UploadAsync([FromForm] UploadDirectoryFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Upload(model));
        }
    }
}
