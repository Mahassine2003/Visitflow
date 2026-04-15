namespace VisitFlowAPI.Models;

public class SafetyMeasure
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AddedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Intervention Intervention { get; set; } = null!;
}
