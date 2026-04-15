namespace VisitFlowAPI.Models;

public class InterventionZone
{
    public int InterventionId { get; set; }
    public int ZoneId { get; set; }

    public Intervention Intervention { get; set; } = null!;
    public Zone Zone { get; set; } = null!;
}
