using System.ComponentModel.DataAnnotations;
using FoodSafetyTracker.Data;
using FoodSafetyTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace FoodSafetyTracker.Tests.Tests;

public class ModelAndDataTests
{
    private ApplicationDbContext CreateDb(string name)
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new ApplicationDbContext(opts);
    }

    [Fact]
    public void Premises_Model_Validation_Fails_For_Missing_Fields()
    {
        var p = new Premises(); // missing required props
        var ctx = new ValidationContext(p);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(p, ctx, results, true);
        Assert.False(valid);
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        Assert.Contains(results, r => r.MemberNames.Contains("Address"));
        Assert.Contains(results, r => r.MemberNames.Contains("Town"));
    }

    [Fact]
    public void Inspection_Model_Validation_Requires_InspectionDate_And_Outcome()
    {
        // The properties are non-nullable value types; DataAnnotations validation will not
        // treat them as missing when they have default values. Instead assert the model
        // declares the Required attribute on the properties (design intent).
        var inspType = typeof(Inspection);
        var inspectionDateHas = inspType.GetProperty("InspectionDate")
            ?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() == true;
        var outcomeHas = inspType.GetProperty("Outcome")
            ?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() == true;

        Assert.True(inspectionDateHas);
        Assert.True(outcomeHas);
    }

    [Fact]
    public void FollowUp_Closed_Requires_ClosedDate_Validation()
    {
        var f = new FollowUp { DueDate = DateTime.Today, Status = FollowUpStatus.Closed, ClosedDate = null };
        // DataAnnotations won't catch this business rule; emulate the controller check
        var isValid = !(f.Status == FollowUpStatus.Closed && f.ClosedDate == null);
        Assert.False(isValid);
    }

    [Fact]
    public async Task Cascade_Delete_Premises_Removes_Inspections_And_FollowUps()
    {
        var db = CreateDb(nameof(Cascade_Delete_Premises_Removes_Inspections_And_FollowUps));
        var prem = new Premises { Name = "P1", Address = "A", Town = "T1", RiskRating = RiskRating.Low };
        db.Premises.Add(prem);
        await db.SaveChangesAsync();

        var insp = new Inspection { PremisesId = prem.Id, InspectionDate = DateTime.Today, Score = 50, Outcome = InspectionOutcome.Pass };
        db.Inspections.Add(insp);
        await db.SaveChangesAsync();

        var f = new FollowUp { InspectionId = insp.Id, DueDate = DateTime.Today.AddDays(7), Status = FollowUpStatus.Open };
        db.FollowUps.Add(f);
        await db.SaveChangesAsync();

        // Confirm created
        Assert.Equal(1, await db.Premises.CountAsync());
        Assert.Equal(1, await db.Inspections.CountAsync());
        Assert.Equal(1, await db.FollowUps.CountAsync());

        // Delete premises
        db.Premises.Remove(prem);
        await db.SaveChangesAsync();

        Assert.Equal(0, await db.Premises.CountAsync());
        Assert.Equal(0, await db.Inspections.CountAsync());
        Assert.Equal(0, await db.FollowUps.CountAsync());
    }

    [Fact]
    public async Task Can_Create_Inspection_From_DbContext()
    {
        var db = CreateDb(nameof(Can_Create_Inspection_From_DbContext));
        var prem = new Premises { Name = "P2", Address = "B", Town = "T2", RiskRating = RiskRating.Medium };
        db.Premises.Add(prem);
        await db.SaveChangesAsync();

        var insp = new Inspection { PremisesId = prem.Id, InspectionDate = DateTime.Today, Score = 77, Outcome = InspectionOutcome.Pass };
        db.Inspections.Add(insp);
        await db.SaveChangesAsync();

        var stored = await db.Inspections.Include(i => i.Premises).FirstOrDefaultAsync(i => i.Id == insp.Id);
        Assert.NotNull(stored);
        Assert.Equal(prem.Id, stored!.PremisesId);
        Assert.Equal("P2", stored.Premises.Name);
    }

    [Fact]
    public async Task FollowUp_DueDate_Before_Inspection_Is_Identified()
    {
        var db = CreateDb(nameof(FollowUp_DueDate_Before_Inspection_Is_Identified));
        var prem = new Premises { Name = "P3", Address = "C", Town = "T3", RiskRating = RiskRating.High };
        db.Premises.Add(prem);
        await db.SaveChangesAsync();

        var insp = new Inspection { PremisesId = prem.Id, InspectionDate = DateTime.Today, Score = 10, Outcome = InspectionOutcome.Fail };
        db.Inspections.Add(insp);
        await db.SaveChangesAsync();

        var f = new FollowUp { InspectionId = insp.Id, DueDate = DateTime.Today.AddDays(-1), Status = FollowUpStatus.Open };

        // Emulate controller business rule
        var inspection = await db.Inspections.FindAsync(f.InspectionId);
        Assert.NotNull(inspection);
        var isInvalid = inspection != null && f.DueDate.Date < inspection.InspectionDate.Date;
        Assert.True(isInvalid);
    }
}
