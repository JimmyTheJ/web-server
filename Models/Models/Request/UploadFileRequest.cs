using Microsoft.AspNetCore.Http;

namespace VueServer.Models.Request
{
    public class UploadFileRequest : DeleteFileModel
    {
        public IFormFile File { get; set; }
    }
}
