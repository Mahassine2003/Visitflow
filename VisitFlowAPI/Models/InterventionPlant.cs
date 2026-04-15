namespace VisitFlowAPI.Models;

public class InterventionPlant
{
    public int InterventionId { get; set; }
    public int PlantId { get; set; }

    public Intervention Intervention { get; set; } = null!;
    public Plant Plant { get; set; } = null!;
}
