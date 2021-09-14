using Microsoft.AspNetCore.Http;

namespace VueServer.Models.Request
{
    public class UploadDirectoryFileRequest : FileModel
    {
        public IFormFile File { get; set; }
    }
}
