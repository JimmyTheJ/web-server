using Microsoft.AspNetCore.Http;

namespace VueServer.Modules.Directory.Models.Request
{
    public class UploadDirectoryFileRequest : FileModel
    {
        public IFormFile File { get; set; }
    }
}
