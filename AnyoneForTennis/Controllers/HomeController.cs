using AnyoneForTennis.Data;
using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AnyoneForTennis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LocalDbContext _context;

        public HomeController(ILogger<HomeController> logger, LocalDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Action for Homepage
        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's ID (using claims-based authentication)
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var viewModel = new HomePageViewModel();

            // If the user is logged in, fetch their UserCoach and UserMember data
            if (userId != null)
            {
                int parsedUserId = int.Parse(userId);

                // Fetch UserCoach data
                var userCoach = await _context.UserCoaches
                    .Include(uc => uc.Coach) // Include the Coach data
                    .Where(uc => uc.UserId == parsedUserId)
                    .ToListAsync();

                // Fetch UserMember data
                var userMember = await _context.UserMembers
                    .Include(um => um.Member) // Include the Member data
                    .Where(um => um.UserId == parsedUserId)
                    .ToListAsync();

                // Assign the fetched data to the ViewModel
                viewModel.UserCoaches = userCoach;
                viewModel.UserMembers = userMember;
            }

            // Pass coaches and schedules for the homepage display
            viewModel.Coaches = await _context.Coach.Take(5).ToListAsync();
            viewModel.Schedules = await _context.Schedule.Include(s => s.SchedulePlus).Take(5).ToListAsync();

            // Check if the user is an admin and pass this information to the view
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            ViewBag.IsAdmin = isAdmin;

            // Return the Homepage view with the ViewModel
            return View("Index", viewModel);
        }

        // Action for Homepage (Main Landing Page)
        public async Task<IActionResult> HomePage()
        {
            var viewModel = new HomePageViewModel
            {
                // Fetch coaches and schedules to display on the landing page
                Coaches = await _context.Coach.Take(5).ToListAsync(),
                Schedules = await _context.Schedule.Include(s => s.SchedulePlus).Take(5).ToListAsync()
            };

            // Check if the user is an admin and pass this information to the view
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            ViewBag.IsAdmin = isAdmin;

            // Return the Index view
            return View(viewModel);
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
