namespace VisitFlowAPI.Models;

public class InterventionPersonnel
{
    public int InterventionId { get; set; }
    public int PersonnelId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Intervention Intervention { get; set; } = null!;
    public Personnel Personnel { get; set; } = null!;
}
