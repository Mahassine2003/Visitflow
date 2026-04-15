namespace VisitFlowAPI.DTOs.Ai;

/// <summary>
/// Résultat de validation IA pour tout document intégrable (images, etc.). Les PDF sont exclus de l’analyse IA.
/// </summary>
public class DocumentValidationResultDto
{
    public bool AiSkipped { get; set; }
    public string? SkipReason { get; set; }
    public bool ValidatedByAI { get; set; }
    public bool IsValid { get; set; }
    public int? Year { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? RawText { get; set; }
}
