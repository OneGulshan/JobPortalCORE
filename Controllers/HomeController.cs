using JobPortalCORE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobPortalCORE.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Mission 1: Jaan-bujh kar error daalna
            //throw new Exception("Bhai, ye mera practice test error hai Kudu seekhne ke liye!");

            return View(); // Ise comment kar diya hai taaki Visual Studio warning na de
        }
        public IActionResult Privacy() => View();
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
