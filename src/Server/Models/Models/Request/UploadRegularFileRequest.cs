using Microsoft.AspNetCore.Http;

namespace VueServer.Models.Request
{
    public class UploadRegularFileRequest
    {
        public IFormFile File { get; set; }
    }
}
