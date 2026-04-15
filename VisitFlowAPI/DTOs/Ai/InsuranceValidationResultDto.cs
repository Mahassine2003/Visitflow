namespace VisitFlowAPI.DTOs.Ai;

public class InsuranceValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Status { get; set; }
    public int? Year { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? RawText { get; set; }
}

