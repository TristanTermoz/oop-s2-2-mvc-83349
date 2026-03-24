namespace FoodSafetyTracker.Models;

public class DashboardViewModel
{
    public int InspectionsThisMonth { get; set; }
    public int FailedThisMonth { get; set; }
    public int OverdueOpenFollowUps { get; set; }
    public string? FilterTown { get; set; }
    public RiskRating? FilterRisk { get; set; }
    public List<string> Towns { get; set; } = new();
}
