using Microsoft.AspNetCore.Http;

namespace VueServer.Modules.Core.Models.Request
{
    public class UploadRegularFileRequest
    {
        public IFormFile File { get; set; }
    }
}
