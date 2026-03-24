using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers
{
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PremisesController> _logger;

        public PremisesController(ApplicationDbContext db,
                                  ILogger<PremisesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: Premises
        public async Task<IActionResult> Index()
        {
            return View(await _db.Premises.ToListAsync());
        }

        // GET: Premises/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var premises = await _db.Premises
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null) return NotFound();

            return View(premises);
        }

        // GET: Premises/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Premises/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Town,RiskRating")] Premises model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Premises.Add(model);
            await _db.SaveChangesAsync();

            // LOG EVENT 1
            _logger.LogInformation(
                "Premises created: {PremisesId} {Name} by {User}",
                model.Id, model.Name, User.Identity!.Name);

            return RedirectToAction(nameof(Index));
        }

        // GET: Premises/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var premises = await _db.Premises.FindAsync(id);
            if (premises == null) return NotFound();

            return View(premises);
        }

        // POST: Premises/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid) return View(model);

            try
            {
                _db.Update(model);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Premises {PremisesId} updated by {User}",
                    model.Id, User.Identity!.Name);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PremisesExists(model.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Premises/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var premises = await _db.Premises
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null) return NotFound();

            return View(premises);
        }

        // POST: Premises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var premises = await _db.Premises.FindAsync(id);
            if (premises != null)
            {
                _db.Premises.Remove(premises);
                await _db.SaveChangesAsync();

                _logger.LogWarning(
                    "Premises {PremisesId} deleted by {User}",
                    id, User.Identity!.Name);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PremisesExists(int id)
        {
            return _db.Premises.Any(e => e.Id == id);
        }
    }
}