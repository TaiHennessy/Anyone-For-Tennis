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
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var viewModel = new HomePageViewModel
            {
                Enrollments = new List<Enrollment>(), // Initialize with an empty list
                RegisteredMembers = new List<Member>() // Also initialize RegisteredMembers
            };

            if (userId != null)
            {
                int parsedUserId = int.Parse(userId);

                // Fetch UserCoach data (for logged-in coach)
                var userCoach = await _context.UserCoaches
                    .Include(uc => uc.Coach)
                    .Where(uc => uc.UserId == parsedUserId)
                    .ToListAsync();

                // Fetch UserMember data (for logged-in members)
                var userMember = await _context.UserMembers
                    .Include(um => um.Member)
                    .Where(um => um.UserId == parsedUserId)
                    .ToListAsync();

                // Fetch SchedulePlus data for the logged-in user's coach
                var schedulePluses = new List<SchedulePlus>();

                if (userCoach.Any())
                {
                    var coachIds = userCoach.Select(uc => uc.CoachId).ToList();
                    schedulePluses = await _context.SchedulePlus
                        .Include(sp => sp.Schedule)
                        .Where(sp => coachIds.Contains(sp.CoachId))
                        .ToListAsync();

                    // Fetch Enrollments linked to the schedules
                    var scheduleIds = schedulePluses.Select(sp => sp.ScheduleId).ToList();
                    var enrollments = await _context.Enrollments
                        .Include(e => e.Schedule)
                        .ThenInclude(s => s.SchedulePlus)
                        .Include(e => e.Member)
                        .Where(e => scheduleIds.Contains(e.ScheduleId))
                        .ToListAsync();

                    viewModel.Enrollments = enrollments;
                }

                viewModel.UserCoaches = userCoach;
                viewModel.UserMembers = userMember;
                viewModel.SchedulePluses = schedulePluses;
            }

            // Return the Homepage view with the ViewModel
            return View(viewModel); // Ensure you are using the "Index" view in your Views/Home folder
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
