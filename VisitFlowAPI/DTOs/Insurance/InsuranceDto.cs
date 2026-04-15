namespace VisitFlowAPI.DTOs.Insurance;

public class InsuranceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public bool IsValid { get; set; }
    public int PersonnelId { get; set; }
    public DateTime CreatedAt { get; set; }
}
