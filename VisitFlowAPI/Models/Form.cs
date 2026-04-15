namespace VisitFlowAPI.Models;

public class Form
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string Type { get; set; } = string.Empty;

    public Document Document { get; set; } = null!;
    public ICollection<InterventionElement> InterventionElements { get; set; } = new List<InterventionElement>();
}
