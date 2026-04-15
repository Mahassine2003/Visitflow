namespace VisitFlowAPI.Models;

public class InterventionElement
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public int? FormId { get; set; }
    public int ElementTypeId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public ElementContext Context { get; set; } = ElementContext.Intervention;

    /// <summary>JSON object: optionId (string) → valeur saisie pour les champs liés au type.</summary>
    public string? FieldValuesJson { get; set; }

    public Intervention Intervention { get; set; } = null!;
    public Form? Form { get; set; }
    public ElementType ElementType { get; set; } = null!;
}
