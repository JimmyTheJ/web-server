using Microsoft.AspNetCore.Http;

namespace VueServer.Models
{
    public class UploadFileRequest
    {
        public IFormFile File { get; set; }

        public string Name { get; set; }
    }
}
