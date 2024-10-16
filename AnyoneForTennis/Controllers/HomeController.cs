using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AnyoneForTennis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Homepage()
        {
            return View();
        }

        public IActionResult Index()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            ViewBag.IsAdmin = isAdmin; // Pass admin status to the view
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: /Home/Logout
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            return View(); // This will render the Logout.cshtml view located in Views/Home
        }


    }
}
