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

        //Schedule View Pages
        //Schedule Page
        public async Task<IActionResult> GetSchedule()
        {
            return View(await _localContext.Schedule.ToListAsync());
        }

        //Coach Schedule Page - with Authorization once the roles are set up
        public IActionResult CoachSchedule()
        {
            return View();
        }


        // Admin Pages
        // GET: Schedules - Admin Control Page
        // Using a view model both the local and main schedules can be displayed
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
        // Local boolean to deal with ID from main being used for local detail
        public async Task<IActionResult> Details(int? id, bool isLocal = false)
        {
            // If ID for both is null
            if (id == null)
            {
                return NotFound();
            }

            if (isLocal)
            {
                // Local
                var scheduleLocal = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleLocal != null)
                {
                    return View(scheduleLocal);
                }
            }
            else
            {
                // Main
                var scheduleMain = await _context.Schedules
                    .FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleMain != null)
                {
                    return View(scheduleMain);
                }
            }
            // if Neither
            return NotFound();
        }


        // GET: Schedules/Create
        public IActionResult Create()
        {
            ViewBag.Locations = Schedule.GetLocations();
            return View();
        }

        // Can't write to main, only local, as main is a read-only
        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,Description,SchedulePlus.DateTime")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                schedule.SchedulePlus = new SchedulePlus
                {
                    // Dont remember what duration was for.
                    DateTime = schedule.SchedulePlus.DateTime,
                    Duration = 1
                };
                _localContext.Add(schedule);
                await _localContext.SaveChangesAsync();
                return RedirectToAction(nameof(ControlPanel));
            }
            return View(schedule);
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
                // Local
                var scheduleLocal = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleLocal != null)
                {
                    ViewData["isLocal"] = isLocal;
                    return View(scheduleLocal);
                }
            }
            else
            {
                // Main
                var scheduleMain = await _context.Schedules
                    .FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleMain != null)
                {
                    ViewData["isLocal"] = isLocal;
                    return View(scheduleMain);
                }
            }
            // if Neither
            return NotFound();
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*[HttpPost]
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
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, bool isLocal, [Bind("ScheduleId,Name,Location,Description")] Schedule schedule)
        {
            if (id != schedule.ScheduleId)
            {
                return NotFound();
            }

            Console.WriteLine("ModelState.IsValid: " + ModelState.IsValid);

            if (ModelState.IsValid)
            {
                try
                {
                    if (isLocal)
                    {
                        Console.WriteLine("Editing local schedule");
                        _localContext.Update(schedule);
                        await _localContext.SaveChangesAsync();
                        Console.WriteLine("Local schedule updated");
                    }
                    else
                    {
                        Console.WriteLine("Editing main schedule");
                        _context.Update(schedule);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Main schedule updated");
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine("DbUpdateConcurrencyException: " + ex.Message);
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
            else
            {
                Console.WriteLine("ModelState is not valid");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine("Error: " + error.ErrorMessage);
                    }
                }
            }

            ViewBag.Locations = Schedule.GetLocations();
            return View(schedule);
        }



        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id, bool isLocal)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (isLocal)
            {
                // Local
                var scheduleLocal = await _localContext.Schedule.FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleLocal != null)
                {
                    ViewData["isLocal"] = true;
                    return View(scheduleLocal);
                }
            }
            else
            {
                // Main
                var scheduleMain = await _context.Schedules
                    .FirstOrDefaultAsync(m => m.ScheduleId == id);
                if (scheduleMain != null)
                {
                    ViewData["isLocal"] = false;
                    return View(scheduleMain);
                }
            }
            // if Neither
            return NotFound();
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool isLocal)
        {
            if (isLocal)
            {
                var scheduleLocal = await _localContext.Schedule.FindAsync(id);
                if (scheduleLocal != null)
                {
                    _localContext.Schedule.Remove(scheduleLocal);
                    await _localContext.SaveChangesAsync();
                }
            }
            else
            {
                // Delete is not allowed on the main database
                Console.WriteLine("Deletion attempted on main database but not allowed");
                return RedirectToAction(nameof(ControlPanel));
            }

            return RedirectToAction(nameof(ControlPanel));
        }


        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.ScheduleId == id);
        }
    }
}
