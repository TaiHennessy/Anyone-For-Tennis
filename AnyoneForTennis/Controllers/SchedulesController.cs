using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Data;
using AnyoneForTennis.Models;

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

        // Schedule View Pages
        // Schedule Page
        public async Task<IActionResult> GetSchedule()
        {
            return View(await _localContext.Schedule.ToListAsync());
        }

        // Coach Schedule Page - with Authorization once the roles are set up
        public IActionResult CoachSchedule()
        {
            return View();
        }

        // Admin Pages
        // GET: Schedules - Admin Control Page
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
            if (id == null)
            {
                return NotFound();
            }

            if (isLocal)
            {
                var scheduleLocal = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleLocal != null)
                {
                    return View(scheduleLocal);
                }
            }
            else
            {
                var scheduleMain = await _context.Schedules.FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleMain != null)
                {
                    return View(scheduleMain);
                }
            }

            return NotFound();
        }

        // GET: Schedules/Create
        public IActionResult Create()
        {
            /* ViewBag.Locations = Schedule.GetLocations();
             ViewBag.Coaches = GetCoaches();
             return View();*/

            var viewModel = new SchedulesViewModel
            {
                Schedule = new Schedule(),
                SchedulePlus = new SchedulePlus()
            };
            ViewBag.Locations = Schedule.GetLocations();
            ViewBag.Coaches = GetCoaches();
            return View(viewModel);
        }

        // POST: Schedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchedulesViewModel viewModel, int selectedCoachId)
        {
            if (ModelState.IsValid)
            {

                var schedule = viewModel.Schedule;

                if (viewModel.SchedulePlus == null)
                {
                    viewModel.SchedulePlus = new SchedulePlus();
                }

                // Explicitly parse the DateTime from the form data
                if (DateTime.TryParse(Request.Form["SchedulePlus.DateTime"], out DateTime parsedDateTime))
                {
                    viewModel.SchedulePlus.DateTime = parsedDateTime;
                }
                else
                {
                    // Handle the case where parsing fails
                    ModelState.AddModelError("", "Invalid DateTime provided.");
                    ViewBag.Locations = Schedule.GetLocations();
                    ViewBag.Coaches = GetCoaches();
                    return View(viewModel);
                }

                // Validate and set CoachId
                if (selectedCoachId > 0)
                {
                    var coachExists = _localContext.Coach.Any(c => c.CoachId == selectedCoachId);
                    if (coachExists)
                    {
                        viewModel.SchedulePlus.CoachId = selectedCoachId;
                    }
                    else
                    {
                        ModelState.AddModelError("", "The selected coach does not exist.");
                        ViewBag.Locations = Schedule.GetLocations();
                        ViewBag.Coaches = GetCoaches();
                        return View(viewModel);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "You must select a coach.");
                    ViewBag.Locations = Schedule.GetLocations();
                    ViewBag.Coaches = GetCoaches();
                    return View(viewModel);
                }

                viewModel.Schedule.SchedulePlus = viewModel.SchedulePlus;
                _localContext.Add(viewModel.Schedule);
                await _localContext.SaveChangesAsync();
                return RedirectToAction(nameof(ControlPanel));
            }

            ViewBag.Locations = Schedule.GetLocations();
            ViewBag.Coaches = GetCoaches();
            return View(viewModel);
        }

        private List<SelectListItem> GetCoaches()
        {
            return _localContext.Coach
                .Select(c => new SelectListItem
                {
                    Value = c.CoachId.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                }).ToList();
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id, bool isLocal = false)
        {
            ViewBag.Locations = Schedule.GetLocations();

            if (id == null)
            {
                return NotFound();
            }

            if (isLocal)
            {
                var scheduleLocal = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleLocal != null)
                {
                    ViewData["isLocal"] = isLocal;
                    return View(scheduleLocal);
                }
            }
            else
            {
                var scheduleMain = await _context.Schedules.FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleMain != null)
                {
                    ViewData["isLocal"] = isLocal;
                    return View(scheduleMain);
                }
            }

            return NotFound();
        }

        // POST: Schedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, bool isLocal, [Bind("ScheduleId,Name,Location,Description")] Schedule schedule)
        {
            if (id != schedule.ScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (isLocal)
                    {
                        _localContext.Update(schedule);
                        await _localContext.SaveChangesAsync();
                    }
                    else
                    {
                        _context.Update(schedule);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.ScheduleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ControlPanel));
            }

            ViewBag.Locations = Schedule.GetLocations();
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scheduleLocal = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (scheduleLocal != null)
            {
                ViewData["isLocal"] = true;
                return View(scheduleLocal);
            }
            return NotFound();
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var scheduleLocal = await _localContext.Schedule.FindAsync(id);
            if (scheduleLocal != null)
            {
                _localContext.Schedule.Remove(scheduleLocal);
                await _localContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ControlPanel));
        }

        private bool ScheduleExists(int id)
        {
            return _localContext.Schedule.Any(e => e.ScheduleId == id);
        }
    }
}
