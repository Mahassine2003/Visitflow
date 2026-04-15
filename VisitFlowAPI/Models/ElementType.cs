namespace VisitFlowAPI.Models;

public class ElementType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ElementOption> ElementOptions { get; set; } = new List<ElementOption>();
    public ICollection<InterventionElement> InterventionElements { get; set; } = new List<InterventionElement>();
}
