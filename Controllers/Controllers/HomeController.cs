using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace VueServer.Controllers
{
    public class HomeController : Controller
    {
        public HomeController() { }

        public IActionResult Index()
        {
            return View("index");
        }

        public IActionResult Error()
        {
            return View("error");
        }

        //[HttpGet]
        //[Authorize(Roles = "Administrator, Elevated, User")]
        ////[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Administrator, Elevated, User")]
        //public IActionResult DownloadFile(string fileName)
        //{
        //    if (String.IsNullOrWhiteSpace(fileName))
        //        return BadRequest();

        //    FileInfo file;
        //    var filepath = Path.Combine(_wwwRoot, @"\videos\", fileName);
        //    //Console.WriteLine("Filepath: " + filepath);

        //    string contentType = MimeTypeHelper.GetMimeType(filepath);
        //    // This is to see if the file exists. It's not technically necessary, but it means we can log that the file isn't where it's supposed to be
        //    try
        //    {
        //        file = new FileInfo(filepath);
        //    }
        //    catch(Exception)
        //    {
        //        _logger.LogWarning("File access error, file either doesn't exist, or is inaccessible for some reason: File download attempt by " + _user.Name + " @ " + _user.IP + " - Filename=" + fileName);
        //        return BadRequest();
        //    }

        //    _logger.LogInformation("Public FileServer download begun by " + _user.Name + " @ " + _user.IP + " - Filename=" + fileName);
        //    return File(filepath, contentType, fileName);
        //}
    }
}
