namespace VisitFlowAPI.Models;

public class BlacklistRequest
{
    public int Id { get; set; }
    public string Reason { get; set; } = string.Empty;
    public BlacklistStatus Status { get; set; } = BlacklistStatus.Pending;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int PersonnelId { get; set; }
    public int? ReviewedByUserId { get; set; }

    public Personnel Personnel { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}
