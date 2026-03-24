using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyTracker.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await db.Database.MigrateAsync();

        // ── Roles ──
        foreach (var role in new[] { "Admin", "Inspector", "Viewer" })
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));

        // ── Admin user ──
        if (await users.FindByEmailAsync("admin@tracker.ie") is null)
        {
            var admin = new IdentityUser
            {
                UserName = "admin@tracker.ie",
                Email = "admin@tracker.ie",
                EmailConfirmed = true
            };
            await users.CreateAsync(admin, "Admin@1234");
            await users.AddToRoleAsync(admin, "Admin");
        }

        // ── Inspector user ──
        if (await users.FindByEmailAsync("inspector@tracker.ie") is null)
        {
            var ins = new IdentityUser
            {
                UserName = "inspector@tracker.ie",
                Email = "inspector@tracker.ie",
                EmailConfirmed = true
            };
            await users.CreateAsync(ins, "Inspector@1234");
            await users.AddToRoleAsync(ins, "Inspector");
        }

        // ── Viewer user ──
        if (await users.FindByEmailAsync("viewer@tracker.ie") is null)
        {
            var v = new IdentityUser
            {
                UserName = "viewer@tracker.ie",
                Email = "viewer@tracker.ie",
                EmailConfirmed = true
            };
            await users.CreateAsync(v, "Viewer@1234");
            await users.AddToRoleAsync(v, "Viewer");
        }

        if (await db.Premises.AnyAsync()) return; // already seeded

        // ── 12 Premises across 3 towns ──
        var premises = new List<Premises>
        {
            // Dorchester
            new() { Name="The Old Mill Café",     Address="1 Mill Lane",      Town="Dorchester", RiskRating=RiskRating.High },
            new() { Name="Riverside Diner",       Address="14 River Rd",      Town="Dorchester", RiskRating=RiskRating.Medium },
            new() { Name="Sunny Bakery",          Address="22 High St",       Town="Dorchester", RiskRating=RiskRating.Low },
            new() { Name="Harbour Fish & Chips",  Address="3 Harbour View",   Town="Dorchester", RiskRating=RiskRating.High },
            // Weymouth
            new() { Name="Seafront Grill",        Address="7 Esplanade",      Town="Weymouth",   RiskRating=RiskRating.Medium },
            new() { Name="The Lobster Pot",       Address="19 Pier St",       Town="Weymouth",   RiskRating=RiskRating.High },
            new() { Name="Bay Bites",             Address="5 Bay Rd",         Town="Weymouth",   RiskRating=RiskRating.Low },
            new() { Name="Marina Café",           Address="12 Marina Walk",   Town="Weymouth",   RiskRating=RiskRating.Low },
            // Bridport
            new() { Name="West Bay Bistro",       Address="2 West Bay Rd",    Town="Bridport",   RiskRating=RiskRating.Medium },
            new() { Name="The Rope & Anchor",     Address="8 Rope Walk",      Town="Bridport",   RiskRating=RiskRating.High },
            new() { Name="Bridport Bakes",        Address="33 South St",      Town="Bridport",   RiskRating=RiskRating.Low },
            new() { Name="Lyme Bay Tearoom",      Address="45 East St",       Town="Bridport",   RiskRating=RiskRating.Medium },
        };
        db.Premises.AddRange(premises);
        await db.SaveChangesAsync();

        // ── 25 Inspections ──
        var now = DateTime.Today;
        var inspections = new List<Inspection>
        {
            new() { PremisesId=premises[0].Id, InspectionDate=now.AddDays(-5),  Score=45, Outcome=InspectionOutcome.Fail, Notes="Rodent evidence found." },
            new() { PremisesId=premises[0].Id, InspectionDate=now.AddDays(-60), Score=78, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[1].Id, InspectionDate=now.AddDays(-10), Score=88, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[1].Id, InspectionDate=now.AddDays(-90), Score=55, Outcome=InspectionOutcome.Fail, Notes="Temperature control issues." },
            new() { PremisesId=premises[2].Id, InspectionDate=now.AddDays(-3),  Score=95, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[2].Id, InspectionDate=now.AddDays(-120),Score=92, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[3].Id, InspectionDate=now.AddDays(-7),  Score=40, Outcome=InspectionOutcome.Fail, Notes="Unsafe storage of raw meat." },
            new() { PremisesId=premises[3].Id, InspectionDate=now.AddDays(-180),Score=72, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[4].Id, InspectionDate=now.AddDays(-2),  Score=80, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[4].Id, InspectionDate=now.AddDays(-100),Score=60, Outcome=InspectionOutcome.Fail },
            new() { PremisesId=premises[5].Id, InspectionDate=now.AddDays(-14), Score=35, Outcome=InspectionOutcome.Fail, Notes="No hot water in kitchen." },
            new() { PremisesId=premises[5].Id, InspectionDate=now.AddDays(-200),Score=85, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[6].Id, InspectionDate=now.AddDays(-20), Score=90, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[7].Id, InspectionDate=now.AddDays(-8),  Score=76, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[7].Id, InspectionDate=now.AddDays(-150),Score=50, Outcome=InspectionOutcome.Fail },
            new() { PremisesId=premises[8].Id, InspectionDate=now.AddDays(-1),  Score=88, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[8].Id, InspectionDate=now.AddDays(-45), Score=72, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[9].Id, InspectionDate=now.AddDays(-6),  Score=30, Outcome=InspectionOutcome.Fail, Notes="Critical hygiene failure." },
            new() { PremisesId=premises[9].Id, InspectionDate=now.AddDays(-70), Score=68, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[10].Id,InspectionDate=now.AddDays(-4),  Score=97, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[10].Id,InspectionDate=now.AddDays(-130),Score=88, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[11].Id,InspectionDate=now.AddDays(-12), Score=62, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[11].Id,InspectionDate=now.AddDays(-80), Score=44, Outcome=InspectionOutcome.Fail },
            new() { PremisesId=premises[0].Id, InspectionDate=now.AddDays(-25), Score=70, Outcome=InspectionOutcome.Pass },
            new() { PremisesId=premises[3].Id, InspectionDate=now.AddDays(-30), Score=55, Outcome=InspectionOutcome.Fail },
        };
        db.Inspections.AddRange(inspections);
        await db.SaveChangesAsync();

        // ── 10 FollowUps ──
        var followUps = new List<FollowUp>
        {
            // Overdue + Open
            new() { InspectionId=inspections[0].Id,  DueDate=now.AddDays(-3),   Status=FollowUpStatus.Open },
            new() { InspectionId=inspections[6].Id,  DueDate=now.AddDays(-1),   Status=FollowUpStatus.Open },
            new() { InspectionId=inspections[10].Id, DueDate=now.AddDays(-7),   Status=FollowUpStatus.Open },
            new() { InspectionId=inspections[17].Id, DueDate=now.AddDays(-5),   Status=FollowUpStatus.Open },
            new() { InspectionId=inspections[24].Id, DueDate=now.AddDays(-2),   Status=FollowUpStatus.Open },
            // Future + Open
            new() { InspectionId=inspections[3].Id,  DueDate=now.AddDays(14),   Status=FollowUpStatus.Open },
            new() { InspectionId=inspections[9].Id,  DueDate=now.AddDays(7),    Status=FollowUpStatus.Open },
            // Closed
            new() { InspectionId=inspections[14].Id, DueDate=now.AddDays(-20),  Status=FollowUpStatus.Closed, ClosedDate=now.AddDays(-15) },
            new() { InspectionId=inspections[22].Id, DueDate=now.AddDays(-30),  Status=FollowUpStatus.Closed, ClosedDate=now.AddDays(-28) },
            new() { InspectionId=inspections[3].Id,  DueDate=now.AddDays(-50),  Status=FollowUpStatus.Closed, ClosedDate=now.AddDays(-45) },
        };
        db.FollowUps.AddRange(followUps);
        await db.SaveChangesAsync();
    }
}
