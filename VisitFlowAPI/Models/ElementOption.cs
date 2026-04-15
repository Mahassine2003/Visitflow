namespace VisitFlowAPI.Models;

public class ElementOption
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int ElementTypeId { get; set; }

    public ElementType ElementType { get; set; } = null!;
}
