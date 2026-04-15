namespace VisitFlowAPI.Models;

public class Visit
{
    public int Id { get; set; }
    public string VisitId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
}
