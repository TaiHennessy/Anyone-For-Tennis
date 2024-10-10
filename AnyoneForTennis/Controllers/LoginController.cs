using Microsoft.AspNetCore.Mvc;
using AnyoneForTennis.Data;
using AnyoneForTennis.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace AnyoneForTennis.Controllers
{
    public class LoginController : Controller
    {
        private readonly LocalDbContext _context;

        public LoginController(LocalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Redirect to the /Views/Home/Login.cshtml view
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (ModelState.IsValid)
            {
                // Find user by username and password
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    // Store user info in session
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                    // User authenticated, redirect to Home/Index
                    TempData["Message"] = "Login Successful!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid username or password.";
                    return View("~/Views/Home/Login.cshtml");
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }
    }
}
