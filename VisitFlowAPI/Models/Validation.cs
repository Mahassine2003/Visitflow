namespace VisitFlowAPI.Models;

public class Validation
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public bool IsApproved { get; set; }
    public string? Comment { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public string ValidatedByRole { get; set; } = string.Empty;

    public Intervention Intervention { get; set; } = null!;
}
