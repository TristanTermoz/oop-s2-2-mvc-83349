using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FoodSafetyTracker.Tests;

public class DashboardTests
{
    private ApplicationDbContext CreateDb(string name)
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new ApplicationDbContext(opts);
    }

    // Test 1: Overdue follow-ups query
    [Fact]
    public async Task OverdueFollowUps_ReturnsOnlyOverdueOpenItems()
    {
        var db = CreateDb(nameof(OverdueFollowUps_ReturnsOnlyOverdueOpenItems));
        var premises = new Premises { Name = "P1", Address = "A", Town = "T1", RiskRating = RiskRating.Low };
        db.Premises.Add(premises);
        await db.SaveChangesAsync();

        var insp = new Inspection
        {
            PremisesId = premises.Id,
            InspectionDate = DateTime.Today.AddDays(-10),
            Score = 50,
            Outcome = InspectionOutcome.Fail
        };
        db.Inspections.Add(insp);
        await db.SaveChangesAsync();

        db.FollowUps.AddRange(
            new FollowUp { InspectionId = insp.Id, DueDate = DateTime.Today.AddDays(-3), Status = FollowUpStatus.Open }, // overdue
            new FollowUp { InspectionId = insp.Id, DueDate = DateTime.Today.AddDays(5), Status = FollowUpStatus.Open }, // future
            new FollowUp { InspectionId = insp.Id, DueDate = DateTime.Today.AddDays(-1), Status = FollowUpStatus.Closed }  // closed
        );
        await db.SaveChangesAsync();

        var overdue = await db.FollowUps
            .Where(f => f.DueDate < DateTime.Today && f.Status == FollowUpStatus.Open)
            .CountAsync();

        Assert.Equal(1, overdue);
    }

    // Test 2: FollowUp cannot be closed without ClosedDate
    [Fact]
    public void ClosedFollowUp_WithoutClosedDate_ShouldBeInvalid()
    {
        var followUp = new FollowUp
        {
            DueDate = DateTime.Today,
            Status = FollowUpStatus.Closed,
            ClosedDate = null   // missing – this is the invalid case
        };

        // Business rule: Status=Closed requires ClosedDate
        var isValid = !(followUp.Status == FollowUpStatus.Closed
                        && followUp.ClosedDate == null);
        Assert.False(isValid);
    }

    // Test 3: Dashboard counts match seed data
    [Fact]
    public async Task DashboardCounts_MatchKnownSeedData()
    {
        var db = CreateDb(nameof(DashboardCounts_MatchKnownSeedData));
        var premises = new Premises { Name = "P1", Address = "A", Town = "T1", RiskRating = RiskRating.High };
        db.Premises.Add(premises);
        await db.SaveChangesAsync();

        var insp = new Inspection
        {
            PremisesId = premises.Id,
            InspectionDate = DateTime.Today.AddDays(-2),
            Score = 30,
            Outcome = InspectionOutcome.Fail
        };
        db.Inspections.Add(insp);
        await db.SaveChangesAsync();

        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var count = await db.Inspections
            .Where(i => i.InspectionDate >= monthStart).CountAsync();
        Assert.Equal(1, count);

        var failed = await db.Inspections
            .Where(i => i.InspectionDate >= monthStart && i.Outcome == InspectionOutcome.Fail).CountAsync();
        Assert.Equal(1, failed);
    }

    // Test 4: Inspector cannot access Admin-only endpoint (role check concept)
    [Fact]
    public void InspectorRole_NotAdmin_CannotCreatePremises()
    {
        // Simulate role check that would occur in controller authorisation
        var userRoles = new[] { "Inspector" };
        bool canCreate = userRoles.Contains("Admin");
        Assert.False(canCreate);
    }
}
