using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace VueServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(_env.WebRootPath, "dist", "index.html"), MimeMapping.KnownMimeTypes.Html);
        }
    }
}
