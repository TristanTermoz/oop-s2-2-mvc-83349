using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;

namespace FoodSafetyTracker.Controllers
{
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: FollowUps
        public async Task<IActionResult> Index()
        {
            var followUps = _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises);
            return View(await followUps.ToListAsync());
        }

        // GET: FollowUps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        // GET: FollowUps/Create
        [Authorize(Roles = "Admin,Inspector")]
        public IActionResult Create()
        {
            var inspections = _context.Inspections
                .Include(i => i.Premises)
                .Select(i => new { i.Id, Text = i.Premises.Name + " - " + i.InspectionDate.ToString("yyyy-MM-dd") });
            ViewData["InspectionId"] = new SelectList(inspections, "Id", "Text");
            return View();
        }

        // POST: FollowUps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            // Business rule: DueDate cannot be before the inspection date
            var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
            if (inspection != null && followUp.DueDate.Date < inspection.InspectionDate.Date)
            {
                // LOG EVENT 5
                _logger.LogWarning(
                    "FollowUp DueDate {DueDate} is before InspectionDate {InspectionDate} for Inspection {InspectionId}",
                    followUp.DueDate, inspection.InspectionDate, followUp.InspectionId);

                ModelState.AddModelError(nameof(followUp.DueDate),
                    "Due date cannot be before the inspection date.");
            }

            // Business rule: if Status is Closed, ClosedDate is required
            if (followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null)
            {
                // LOG EVENT 7
                _logger.LogWarning(
                    "Attempt to close FollowUp without a ClosedDate by {User}",
                    User.Identity!.Name);

                ModelState.AddModelError(nameof(followUp.ClosedDate),
                    "A closed date is required when status is Closed.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(followUp);
                await _context.SaveChangesAsync();

                // LOG EVENT 6
                _logger.LogInformation(
                    "FollowUp {FollowUpId} created for Inspection {InspectionId} by {User}",
                    followUp.Id, followUp.InspectionId, User.Identity!.Name);

                return RedirectToAction(nameof(Index));
            }

            var inspections = _context.Inspections
                .Include(i => i.Premises)
                .AsEnumerable()
                .Select(i => new { i.Id, Text = i.Premises.Name + " - " + i.InspectionDate.ToString("yyyy-MM-dd") });
            ViewData["InspectionId"] = new SelectList(inspections, "Id", "Text", followUp.InspectionId);
            if (!ModelState.IsValid)
            {
                foreach (var kv in ModelState)
                {
                    foreach (var err in kv.Value.Errors)
                        _logger.LogWarning("ModelState error for {Key}: {Error}", kv.Key, err.ErrorMessage);
                }
            }

            return View(followUp);
        }

        // GET: FollowUps/Edit/5
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null) return NotFound();
            var inspections = _context.Inspections
                .Include(i => i.Premises)
                .Select(i => new { i.Id, Text = i.Premises.Name + " - " + i.InspectionDate.ToString("yyyy-MM-dd") });
            ViewData["InspectionId"] = new SelectList(inspections, "Id", "Text", followUp.InspectionId);
            return View(followUp);
        }

        // POST: FollowUps/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id) return NotFound();

            // Business rule: if Status is Closed, ClosedDate is required
            if (followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate == null)
            {
                _logger.LogWarning(
                    "Attempt to edit FollowUp {Id} to Closed without ClosedDate by {User}",
                    id, User.Identity!.Name);

                ModelState.AddModelError(nameof(followUp.ClosedDate),
                    "A closed date is required when status is Closed.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followUp);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "FollowUp {FollowUpId} updated by {User}",
                        followUp.Id, User.Identity!.Name);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowUpExists(followUp.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var inspections = _context.Inspections
                .Include(i => i.Premises)
                .Select(i => new { i.Id, Text = i.Premises.Name + " - " + i.InspectionDate.ToString("yyyy-MM-dd") });
            ViewData["InspectionId"] = new SelectList(inspections, "Id", "Text", followUp.InspectionId);
            return View(followUp);
        }

        // GET: FollowUps/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                    .ThenInclude(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null) return NotFound();

            return View(followUp);
        }

        // POST: FollowUps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp != null)
            {
                _context.FollowUps.Remove(followUp);
                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "FollowUp {FollowUpId} deleted by {User}",
                    id, User.Identity!.Name);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }
    }
}