namespace VisitFlowAPI.Models;

public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<InterventionZone> InterventionZones { get; set; } = new List<InterventionZone>();
}
