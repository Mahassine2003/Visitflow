namespace VisitFlowAPI.DTOs.Interventions;

public class InterventionWizardFieldDefinitionDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int FieldType { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
    /// <summary>0 = champ personnalisé, 1 = liaison table/colonne existante.</summary>
    public int CreationMode { get; set; }
    public string? SourceSchema { get; set; }
    public string? SourceTable { get; set; }
    public string? SourceColumn { get; set; }
    /// <summary>Choices for <c>FieldType == Select</c> (dropdown).</summary>
    public string[]? Options { get; set; }
}

public class InterventionWizardFieldCreateDto
{
    /// <summary>0 = nouveau champ (définition en base), 1 = champ lié à une table/colonne.</summary>
    public int CreationMode { get; set; }

    public string Label { get; set; } = string.Empty;
    public int FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int? SortOrder { get; set; }
    /// <summary>Labels for dropdown options when <c>FieldType</c> is Select (mode personnalisé uniquement).</summary>
    public List<string>? FieldOptions { get; set; }

    /// <summary>Si <see cref="CreationMode"/> = 1 : schéma SQL (défaut dbo).</summary>
    public string? SourceSchema { get; set; }
    public string? SourceTable { get; set; }
    public string? SourceColumn { get; set; }
}

public class InterventionWizardFieldUpdateDto
{
    public int CreationMode { get; set; }
    public string Label { get; set; } = string.Empty;
    public int FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public List<string>? FieldOptions { get; set; }
    public string? SourceSchema { get; set; }
    public string? SourceTable { get; set; }
    public string? SourceColumn { get; set; }
}
