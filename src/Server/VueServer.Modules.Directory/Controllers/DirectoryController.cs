using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VueServer.Core.Status;
using VueServer.Modules.Core.Controllers.Filters;
using VueServer.Modules.Core.Services.Account;
using VueServer.Modules.Core.Services.Module;
using VueServer.Modules.Directory.Models;
using VueServer.Modules.Directory.Models.Request;
using VueServer.Modules.Directory.Services;
using static VueServer.Domain.DomainConstants.Authentication;
using Route = VueServer.Modules.Core.Controllers.Constants.API_ENDPOINTS;

namespace VueServer.Modules.Directory.Controllers
{
    [Route(DirectoryConstants.Controller.BasePath)]
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
        [Route(DirectoryConstants.Controller.DownloadProtectedFile)]
        public async Task<IActionResult> DownloadProtectedFile(string filename, string token)
        {
            var validated = _accountService.ValidateTokenAndGetName(token);
            if (validated.Code != Domain.Enums.StatusCode.OK)
            {
                return Unauthorized();
            }

            var userHasModule = await _moduleService.DoesUserHaveModule(validated.Obj, DirectoryConstants.ModuleAddOn.Id);
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
        [Route(DirectoryConstants.Controller.ServeMedia)]
        public async Task<IActionResult> ServeMedia(string filename, string token)
        {
            var validated = _accountService.ValidateTokenAndGetName(token);
            if (validated.Code != Domain.Enums.StatusCode.OK)
            {
                return Unauthorized();
            }

            var userHasModule = await _moduleService.DoesUserHaveModule(validated.Obj, DirectoryConstants.ModuleAddOn.Id);
            if (userHasModule.Obj == false)
            {
                return Unauthorized();
            }

            var userHasFeature = await _moduleService.DoesUserHaveFeature(validated.Obj, DirectoryConstants.ModuleFeatures.VIEWER_ID);
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
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id)]
        [Route(DirectoryConstants.Controller.LoadDirectory)]
        public async Task<IActionResult> LoadDirectory(string directory, string dir = null)
        {
            return _codeFactory.GetStatusCode(await _service.Load(directory, dir));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.CREATE_ID)]
        [Route(DirectoryConstants.Controller.CreateFolder)]
        public async Task<IActionResult> CreateFolder([FromBody] FileModel model)
        {
            return _codeFactory.GetStatusCode(await _service.CreateFolder(model.Directory, model.SubDirectory, model.Name));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.MOVE_ID)]
        [Route(DirectoryConstants.Controller.CopyFile)]
        public async Task<IActionResult> CopyFile([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.CopyFile(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.MOVE_ID)]
        [Route(DirectoryConstants.Controller.CopyFolder)]
        public async Task<IActionResult> CopyFolder([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.CopyFolder(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.MOVE_ID)]
        [Route(DirectoryConstants.Controller.MoveFile)]
        public async Task<IActionResult> MoveFile([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.MoveFile(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.MOVE_ID)]
        [Route(DirectoryConstants.Controller.MoveFolder)]
        public async Task<IActionResult> MoveFolder([FromBody] CopyRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.MoveFolder(model.Source, model.Destination));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.MOVE_ID)]
        [Route(DirectoryConstants.Controller.RenameFile)]
        public async Task<IActionResult> RenameFile([FromBody] MoveFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.RenameFile(model));
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.MOVE_ID)]
        [Route(DirectoryConstants.Controller.RenameFolder)]
        public async Task<IActionResult> RenameFolder([FromBody] MoveFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.RenameFolder(model));
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id)]
        [Route(Route.Generic.List)]
        public async Task<IActionResult> GetDirectoryList()
        {
            return _codeFactory.GetStatusCode(await _service.GetDirectories());
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.DELETE_ID)]
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
        [ModuleAuthFilterFactory(Module = DirectoryConstants.ModuleAddOn.Id, Feature = DirectoryConstants.ModuleFeatures.UPLOAD_ID)]
        [Route(DirectoryConstants.Controller.Upload)]
        public async Task<IActionResult> UploadAsync([FromForm] UploadDirectoryFileRequest model)
        {
            return _codeFactory.GetStatusCode(await _service.Upload(model));
        }
    }
}
