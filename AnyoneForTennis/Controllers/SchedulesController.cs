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
        public async Task<IActionResult> Schedule()
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
                LocalSchedule = await _localContext.Schedule.ToListAsync()
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
            return View();
        }

        // Can't write to main, only local, as main is a read-only
        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,Description")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                _localContext.Add(schedule);
                await _localContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id, bool isLocal = false)
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

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ScheduleId,Name,Location,Description")] Schedule schedule)
        {
            if (id != schedule.ScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id, bool isLocal = false)
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

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.ScheduleId == id);
        }
    }
}
