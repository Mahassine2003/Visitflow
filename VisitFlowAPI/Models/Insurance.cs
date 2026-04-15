namespace VisitFlowAPI.Models;

public class Insurance
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public bool IsValid { get; set; }
    /// <summary>False when the file was attached without AI analysis (manual entry).</summary>
    public bool ValidatedByAi { get; set; } = true;
    public int PersonnelId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Personnel Personnel { get; set; } = null!;
    public ICollection<TypeOfWorkInsurance> TypeOfWorkInsurances { get; set; } = new List<TypeOfWorkInsurance>();
}
