namespace VisitFlowAPI.Models;

public class Session
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public int UserId { get; set; }

    public User User { get; set; } = null!;
}
