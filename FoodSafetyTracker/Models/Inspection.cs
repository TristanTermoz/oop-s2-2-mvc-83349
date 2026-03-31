using System.ComponentModel.DataAnnotations;

namespace FoodSafetyTracker.Models;

public class Inspection
{
    public int Id { get; set; }
    public int PremisesId { get; set; }
    public Premises? Premises { get; set; }

    [Required]
    public DateTime InspectionDate { get; set; }

    [Range(0, 100)]
    public int Score { get; set; }

    [Required]
    public InspectionOutcome Outcome { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}

public enum InspectionOutcome { Pass, Fail }
