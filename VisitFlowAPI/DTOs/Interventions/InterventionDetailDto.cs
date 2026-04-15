namespace VisitFlowAPI.DTOs.Interventions;

public class ElementOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class InterventionElementDto
{
    public int Id { get; set; }
    public int ElementTypeId { get; set; }
    public string ElementTypeName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public string Context { get; set; } = string.Empty;
    /// <summary>Champs liés au type d’élément (options configurées en base).</summary>
    public List<ElementOptionDto> Options { get; set; } = new();
    /// <summary>Valeurs saisies : clé = id d’option (string), valeur = texte.</summary>
    public Dictionary<string, string>? FieldValues { get; set; }
}

public class SafetyMeasureDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AddedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class InterventionDetailDto : InterventionDto
{
    public List<InterventionElementDto> Elements { get; set; } = new();
    public List<SafetyMeasureDto> SafetyMeasures { get; set; } = new();
}

public class AddInterventionElementDto
{
    public int ElementTypeId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    /// <summary>0 = Intervention, 1 = HSE</summary>
    public int Context { get; set; }
}

public class AddSafetyMeasureDto
{
    public string Description { get; set; } = string.Empty;
}

public class UpdateInterventionWorkflowDto
{
    public string? FirePermitDetails { get; set; }
    public string? HeightPermitDetails { get; set; }
    public string? HseFormDetails { get; set; }
    public string? HseComment { get; set; }
}

public class UpdateInterventionElementFieldsDto
{
    public Dictionary<string, string> FieldValues { get; set; } = new();
}
