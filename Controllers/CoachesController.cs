using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Data;
using AnyoneForTennis.Models;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.Extensions.Logging;  // Add this for logging

namespace AnyoneForTennis.Controllers
{
    public class CoachesController : Controller
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<CoachesController> _logger;  // Inject logger

        public CoachesController(LocalDbContext context, ILogger<CoachesController> logger)  // Add logger to constructor
        {
            _context = context;
            _logger = logger;
        }

        // GET: Coaches
        public async Task<IActionResult> Index()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isAdmin = false;
            int? userCoachId = null;

            if (userId != null)
            {
                var user = await _context.Users.FindAsync(int.Parse(userId));
                isAdmin = user?.IsAdmin ?? false;

                var userCoach = await _context.UserCoaches
                    .Where(uc => uc.UserId == int.Parse(userId))
                    .FirstOrDefaultAsync();
                userCoachId = userCoach?.CoachId;
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.UserCoachId = userCoachId;

            var coaches = await _context.Coach.ToListAsync();
            return View(coaches);
        }


        // GET: Coaches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogError("Details: Coach ID is null");
                return NotFound();
            }

            var coach = await _context.Coach
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coach == null)
            {
                _logger.LogError($"Details: Coach with ID {id} not found");
                return NotFound();
            }

            return View(coach);
        }

        // GET: Coaches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coaches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CoachId,FirstName,LastName,Biography,Photo")] Coach coach)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Create: Adding a new coach");
                _context.Add(coach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Create: ModelState is invalid");
            return View(coach);
        }

        // GET: Coaches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogError("Edit: Coach ID is null");
                return NotFound();
            }

            var coach = await _context.Coach.FindAsync(id);
            if (coach == null)
            {
                _logger.LogError($"Edit: Coach with ID {id} not found");
                return NotFound();
            }

            _logger.LogInformation($"Edit: Found coach with ID {id}");
            return View(coach);
        }

        // POST: Coaches/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Coach updatedCoach)
        {
            var coach = await _context.Coach.FindAsync(id);

            if (coach == null)
            {
                _logger.LogError($"Edit: Coach with ID {id} not found");
                return NotFound();
            }

            // Only update the Biography field
            coach.Biography = updatedCoach.Biography;

            // Remove other fields from ModelState validation since we're not updating them
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("Photo");

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation($"Edit: Updating coach with ID {id}");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoachExists(coach.CoachId))
                    {
                        _logger.LogError($"Edit: Concurrency issue - Coach with ID {id} no longer exists");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError($"Edit: Concurrency issue when editing coach with ID {id}");
                        throw;
                    }
                }

                _logger.LogInformation($"Edit: Successfully updated coach with ID {id}");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning($"Edit: ModelState is invalid for coach with ID {id}");
            return View(updatedCoach);
        }


        // GET: Coaches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogError("Delete: Coach ID is null");
                return NotFound();
            }

            var coach = await _context.Coach
                .FirstOrDefaultAsync(m => m.CoachId == id);
            if (coach == null)
            {
                _logger.LogError($"Delete: Coach with ID {id} not found");
                return NotFound();
            }

            return View(coach);
        }

        // POST: Coaches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coach = await _context.Coach.FindAsync(id);
            if (coach != null)
            {
                _logger.LogInformation($"Delete: Removing coach with ID {id}");
                _context.Coach.Remove(coach);
            }
            else
            {
                _logger.LogError($"Delete: Coach with ID {id} not found for deletion");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoachExists(int id)
        {
            return _context.Coach.Any(e => e.CoachId == id);
        }
    }
}
