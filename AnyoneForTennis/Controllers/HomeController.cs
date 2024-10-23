using AnyoneForTennis.Data;
using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using System.Linq;

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
                var enrollments = new List<Enrollment>();

                // If the user is a coach, fetch schedules and related enrollments
                if (userCoach.Any())
                {
                    var coachIds = userCoach.Select(uc => uc.CoachId).ToList();
                    schedulePluses = await _context.SchedulePlus
                        .Include(sp => sp.Schedule)
                        .Where(sp => coachIds.Contains(sp.CoachId))
                        .ToListAsync();

                    // Fetch Enrollments linked to the schedules (for coaches to see who enrolled)
                    var scheduleIds = schedulePluses.Select(sp => sp.ScheduleId).ToList();
                    enrollments = await _context.Enrollments
                        .Include(e => e.Schedule)
                        .ThenInclude(s => s.SchedulePlus)
                        .Include(e => e.Member) // To get the members
                        .Where(e => scheduleIds.Contains(e.ScheduleId))
                        .ToListAsync();

                    viewModel.Enrollments = enrollments;
                }

                // If the user is a member, fetch their enrollments
                if (userMember.Any())
                {
                    var memberIds = userMember.Select(um => um.MemberId).ToList();
                    enrollments = await _context.Enrollments
                        .Include(e => e.Schedule)
                        .ThenInclude(s => s.SchedulePlus)
                        .Include(e => e.Schedule.SchedulePlus.Coach) // To show coach details in the view
                        .Where(e => memberIds.Contains(e.MemberId)) // Get enrollments where the member is logged in
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

        // POST: Home/RemoveEnrollment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEnrollment(int enrollmentId)
        {
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            // Find the UserMember entry for the logged-in user
            var userMember = await _context.UserMembers
                .Include(um => um.Member)
                .FirstOrDefaultAsync(um => um.UserId == int.Parse(userId));

            if (userMember == null)
            {
                // If no UserMember relationship is found for the logged-in user
                return NotFound();
            }

            // Find the enrollment by ID and ensure it belongs to the logged-in user's member
            var enrollment = await _context.Enrollments
                .Include(e => e.Member)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.MemberId == userMember.MemberId);

            if (enrollment == null)
            {
                // Enrollment not found or doesn't belong to this user's member
                return NotFound();
            }

            // Remove the enrollment from the database
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            // Redirect to Index page after successful removal
            return RedirectToAction("Index");
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

        public IActionResult Error()
        {
            // Log the error message if available
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature != null)
            {
                // Log the exception details to the console
                Console.WriteLine($"Error occurred on path: {exceptionFeature.Path}");
                Console.WriteLine($"Error Message: {exceptionFeature.Error.Message}");
            }

            // Return the Error view
            return View();
        }



        // Route for simulating an error
        public IActionResult SimulatedError()
        {
            try
            {
                // Simulate an error by throwing an exception
                throw new InvalidOperationException("This is a simulated error for testing purposes.");
            }
            catch (Exception ex)
            {
                // Log the error to the console
                _logger.LogError(ex, "Simulated error occurred");


                return View("~/Views/Shared/Error.cshtml");
            }
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
