namespace VisitFlowAPI.DTOs.Insurance;

public class InsuranceCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public int PersonnelId { get; set; }
}
