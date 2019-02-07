using Microsoft.AspNetCore.Http;

namespace VueServer.Models
{
    public class UploadFileRequest : DeleteFileModel
    {
        public IFormFile File { get; set; }
    }
}
