namespace VisitFlowAPI.Models;

public class Approved
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public UserRole ApproverRole { get; set; }
    public int RequiredApproversCount { get; set; }
    public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;

    public Intervention Intervention { get; set; } = null!;
}
