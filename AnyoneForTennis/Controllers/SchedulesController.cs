﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Data;
using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Authorization;


namespace AnyoneForTennis.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly Hitdb1Context _context;
        private readonly LocalDbContext _localContext;

        public SchedulesController(Hitdb1Context context, LocalDbContext localContext)
        {
            _context = context;
            _localContext = localContext;
        }

        // Schedule Page
        public async Task<IActionResult> GetSchedule()
        {
            var schedules = await _localContext.Schedule
                .Include(s => s.SchedulePlus)  // Include related SchedulePlus data
                .ThenInclude(sp => sp.Coach)   // Include related Coach data
                .ToListAsync();

            return View(schedules); // Pass the list of schedules to the view
        }

        // Coach Schedule Page
        public IActionResult CoachSchedule()
        {
            return View();
        }

        // Admin Control Page
        // Admin Control Page
        public async Task<IActionResult> ControlPanel()
        {
            var viewModel = new ScheduleViewModel
            {
                MainSchedule = await _context.Schedules.ToListAsync(),
                LocalSchedule = await _localContext.Schedule.ToListAsync(),
                Member = await _context.Members.ToListAsync(),
                Coach = await _context.Coaches.ToListAsync()
            };
            return View(viewModel);
        }

        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(int? id, bool isLocal = false)
        {
            if (id == null) return NotFound();

            var schedule = new Schedule();

            schedule = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
        

            if (schedule == null) return NotFound();

            // Always try to fetch SchedulePlus from the local context
            var schedulePlus = await _localContext.SchedulePlus
                .Include(sp => sp.Coach)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);

            var viewModel = new SchedulesViewModel
            {
                Schedule = schedule,
                SchedulePlus = schedulePlus,
                Coaches = _context.Coaches.ToList()
            };

            ViewBag.Locations = Schedule.GetLocations();
            ViewBag.Coaches = GetCoaches();
            ViewData["isLocal"] = isLocal;

            Console.WriteLine($"Schedule: {schedule.Name}, {schedule.Location}, {schedule.Description}");
            if (schedulePlus != null)
            {
                Console.WriteLine($"SchedulePlus: DateTime={schedulePlus.DateTime}, CoachId={schedulePlus.CoachId}");
                Console.WriteLine($"SchedulePlus Details: SchedulePlusId={schedulePlus.SchedulePlusId}, ScheduleId={schedulePlus.ScheduleId}, DateTime={schedulePlus.DateTime}, Duration={schedulePlus.Duration}, CoachId={schedulePlus.CoachId}");
            }
            else
            {
                Console.WriteLine("SchedulePlus is null.");
            }

            return View(viewModel);
        }


        // GET: Schedules/Create
        public IActionResult Create()
        {
            var viewModel = new SchedulesViewModel
            {
                Schedule = new Schedule(),
                SchedulePlus = new SchedulePlus()
                {
                    DateTime = DateTime.Now, // Default Time instead of 1/1/0001 12:00:00
                },
                Coaches = _context.Coaches.ToList()  // Initialize the list of coaches
            };

            ViewBag.Locations = Schedule.GetLocations();
            ViewBag.Coaches = GetCoaches();
            return View(viewModel);
        }

        // POST: Schedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchedulesViewModel viewModel)
        {
            // Step 1: Remove unnecessary fields from ModelState validation
            Console.WriteLine("==== Debug: Removing navigation properties from ModelState ====");
            ModelState.Remove("Coach.Coach");
            ModelState.Remove("SchedulePlus.Coach");
            ModelState.Remove("SchedulePlus.Schedule");
            ModelState.Remove("Schedule.Enrollments");

            // Step 2: Log ModelState after removing properties
            if (!ModelState.IsValid)
            {
                Console.WriteLine("==== Debug: ModelState is still invalid ====");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Error in {key}: {error.ErrorMessage}");
                    }
                }
                PrepareViewData();
                return View(viewModel);  // Return the view with current data
            }

            try
            {
                Console.WriteLine("==== Debug: ModelState is valid, proceeding to save ====");

                // Step 3: Save Schedule first to generate ScheduleId
                _localContext.Schedule.Add(viewModel.Schedule);
                await _localContext.SaveChangesAsync();

                // Step 4: Link SchedulePlus with Schedule
                var schedulePlus = viewModel.SchedulePlus ?? new SchedulePlus();
                schedulePlus.ScheduleId = viewModel.Schedule.ScheduleId;

                // Parse DateTime from the form data
                if (!DateTime.TryParse(Request.Form["SchedulePlus.DateTime"], out DateTime parsedDateTime))
                {
                    Console.WriteLine("==== Debug: Failed to parse DateTime ====");
                    ModelState.AddModelError("", "Invalid DateTime provided.");
                    PrepareViewData();
                    return View(viewModel);
                }
                schedulePlus.DateTime = parsedDateTime;

                // Validate the CoachId
                if (!ValidateCoach(schedulePlus.CoachId, out var errorMessage))
                {
                    Console.WriteLine($"==== Debug: Coach validation failed: {errorMessage} ====");
                    ModelState.AddModelError("", errorMessage);
                    PrepareViewData();
                    return View(viewModel);
                }

                // Save SchedulePlus
                _localContext.SchedulePlus.Add(schedulePlus);
                await _localContext.SaveChangesAsync();

                Console.WriteLine("==== Debug: Successfully saved Schedule and SchedulePlus ====");
                return RedirectToAction(nameof(ControlPanel));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"==== Debug: Exception occurred: {ex.Message} ====");
                Console.WriteLine($"==== Debug: Stack Trace: {ex.StackTrace} ====");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                PrepareViewData();
                return View(viewModel);
            }
        }

        // Helper Method to Load ViewBag Data
        private void PrepareViewData()
        {
            ViewBag.Locations = Schedule.GetLocations();
            ViewBag.Coaches = GetCoaches();
        }

        // Helper Method to Validate Coach Selection
        private bool ValidateCoach(int coachId, out string errorMessage)
        {
            errorMessage = "";
            if (coachId <= 0)
            {
                errorMessage = "You must select a valid coach.";
                return false;
            }

            if (!_localContext.Coach.Any(c => c.CoachId == coachId))
            {
                errorMessage = "The selected coach does not exist.";
                return false;
            }

            return true;
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var schedule = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (schedule == null) return NotFound();

            var schedulePlus = await _localContext.SchedulePlus.FirstOrDefaultAsync(m => m.ScheduleId == id) ?? new SchedulePlus();

            var viewModel = new SchedulesViewModel
            {
                Schedule = schedule,
                SchedulePlus = schedulePlus,
                Coaches = _context.Coaches.ToList()
            };

            ViewBag.Locations = Schedule.GetLocations();
            ViewBag.Coaches = GetCoaches();

            // Log the locations
            foreach (var location in ViewBag.Locations)
            {
                Console.WriteLine($"Location: {location.Text} - {location.Value}");
            }

            return View(viewModel);
        }


        // POST: Schedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SchedulesViewModel viewModel)
        {
            if (id != viewModel.Schedule.ScheduleId) return NotFound();

            // Remove unnecessary fields from ModelState validation
            ModelState.Remove("Coach.Coach");
            ModelState.Remove("SchedulePlus.Coach");
            ModelState.Remove("SchedulePlus.Schedule");
            ModelState.Remove("Schedule.Enrollments");

            if (ModelState.IsValid)
            {
                try
                {
                    _localContext.Update(viewModel.Schedule);
                    await _localContext.SaveChangesAsync();

                    var schedulePlus = viewModel.SchedulePlus ?? new SchedulePlus();
                    schedulePlus.ScheduleId = viewModel.Schedule.ScheduleId;

                    if (!DateTime.TryParse(Request.Form["SchedulePlus.DateTime"], out var parsedDateTime))
                    {
                        ModelState.AddModelError("", "Invalid DateTime provided.");
                        PrepareViewData();
                        return View(viewModel);
                    }

                    schedulePlus.DateTime = parsedDateTime;

                    if (!ValidateCoach(schedulePlus.CoachId, out var errorMessage))
                    {
                        ModelState.AddModelError("", errorMessage);
                        PrepareViewData();
                        return View(viewModel);
                    }

                    var existingSchedulePlus = await _localContext.SchedulePlus.FirstOrDefaultAsync(sp => sp.ScheduleId == schedulePlus.ScheduleId);
                    if (existingSchedulePlus != null)
                    {
                        existingSchedulePlus.CoachId = schedulePlus.CoachId;
                        existingSchedulePlus.DateTime = schedulePlus.DateTime;
                        _localContext.Update(existingSchedulePlus);
                    }
                    else
                    {
                        _localContext.Add(schedulePlus);
                    }

                    await _localContext.SaveChangesAsync();
                    return RedirectToAction(nameof(ControlPanel));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(viewModel.Schedule.ScheduleId)) return NotFound();
                    throw;
                }
            }

            PrepareViewData();
            return View(viewModel);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id, bool isLocal = false)
        {
            if (id == null) return NotFound();

            var schedule = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);

            if (schedule == null) return NotFound();

            var schedulePlus = isLocal
                ? await _localContext.SchedulePlus
                    .Include(sp => sp.Coach)
                    .FirstOrDefaultAsync(m => m.ScheduleId == id)
                : null;

            var viewModel = new SchedulesViewModel
            {
                Schedule = schedule,
                SchedulePlus = schedulePlus,
                Coaches = _context.Coaches.ToList()
            };

            ViewBag.Locations = Schedule.GetLocations();
            ViewBag.Coaches = GetCoaches();
            ViewData["isLocal"] = true;

            return View(viewModel);
        }


        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _localContext.Schedule.FindAsync(id);
            if (schedule != null)
            {
                // First, delete the related SchedulePlus records
                var schedulePlus = await _localContext.SchedulePlus.Where(sp => sp.ScheduleId == id).ToListAsync();
                _localContext.SchedulePlus.RemoveRange(schedulePlus);

                // Now, delete the Schedule
                _localContext.Schedule.Remove(schedule);
                await _localContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ControlPanel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]  // Only allow logged-in users to enroll
        public async Task<IActionResult> Enroll(int scheduleId)
        {
            // Get the currently logged-in user
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Check if the user is a member (linked to a Member object)
            var userMember = await _localContext.UserMembers.FirstOrDefaultAsync(um => um.UserId == int.Parse(userId));

            if (userMember == null)
            {
                // If the user is not a member, show error
                return Json(new { isSuccess = false, message = "You need to be a member to enroll in a schedule." });
            }

            // Get the selected schedule
            var schedule = await _localContext.Schedule
                .Include(s => s.SchedulePlus)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

            if (schedule == null || schedule.SchedulePlus == null)
            {
                // Invalid schedule
                return Json(new { isSuccess = false, message = "Invalid schedule." });
            }

            // Check if the user is already enrolled in this schedule
            var existingEnrollment = await _localContext.Enrollments
                .FirstOrDefaultAsync(e => e.MemberId == userMember.MemberId && e.ScheduleId == schedule.ScheduleId);

            if (existingEnrollment != null)
            {
                // User is already enrolled
                return Json(new { isSuccess = false, message = "You have already enrolled in this." });
            }

            // Create a new enrollment record
            var enrollment = new Enrollment
            {
                MemberId = userMember.MemberId,
                CoachId = schedule.SchedulePlus.CoachId,
                ScheduleId = schedule.ScheduleId
            };

            _localContext.Enrollments.Add(enrollment);
            await _localContext.SaveChangesAsync();

            // Successfully enrolled
            return Json(new { isSuccess = true, message = "Successfully enrolled." });
        }





        // Helper Method to Check if Schedule Exists
        private bool ScheduleExists(int id)
        {
            return _localContext.Schedule.Any(e => e.ScheduleId == id);
        }

        // Helper Method to Get Coaches List
        private List<SelectListItem> GetCoaches()
        {
            return _localContext.Coach
                .Select(c => new SelectListItem
                {
                    Value = c.CoachId.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                }).ToList();
        }
    }
}
