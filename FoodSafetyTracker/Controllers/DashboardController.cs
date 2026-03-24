using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext db,
                               ILogger<DashboardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? town, RiskRating? risk)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Base query with optional filters
        var insQuery = _db.Inspections
            .Include(i => i.Premises)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(town))
            insQuery = insQuery.Where(i => i.Premises.Town == town);
        if (risk.HasValue)
            insQuery = insQuery.Where(i => i.Premises.RiskRating == risk.Value);

        var inspectionsThisMonth = await insQuery
            .Where(i => i.InspectionDate >= monthStart)
            .CountAsync();

        var failedThisMonth = await insQuery
            .Where(i => i.InspectionDate >= monthStart
                     && i.Outcome == InspectionOutcome.Fail)
            .CountAsync();

        // Overdue follow-ups: DueDate < today AND Status = Open
        var overdueQuery = _db.FollowUps
            .Include(f => f.Inspection).ThenInclude(i => i.Premises)
            .Where(f => f.DueDate < today && f.Status == FollowUpStatus.Open);

        if (!string.IsNullOrWhiteSpace(town))
            overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.Town == town);
        if (risk.HasValue)
            overdueQuery = overdueQuery.Where(f => f.Inspection.Premises.RiskRating == risk.Value);

        var overdueCount = await overdueQuery.CountAsync();

        var vm = new DashboardViewModel
        {
            InspectionsThisMonth = inspectionsThisMonth,
            FailedThisMonth = failedThisMonth,
            OverdueOpenFollowUps = overdueCount,
            FilterTown = town,
            FilterRisk = risk,
            Towns = await _db.Premises.Select(p => p.Town).Distinct().ToListAsync()
        };

        _logger.LogInformation(
            "Dashboard viewed by {User} – Town:{Town} Risk:{Risk}",
            User.Identity!.Name, town ?? "all", risk?.ToString() ?? "all");

        return View(vm);
    }
}
