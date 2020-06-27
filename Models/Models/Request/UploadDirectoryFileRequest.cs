using Microsoft.AspNetCore.Http;

namespace VueServer.Models.Request
{
    public class UploadDirectoryFileRequest : DeleteFileModel
    {
        public IFormFile File { get; set; }
    }
}
