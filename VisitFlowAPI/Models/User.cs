namespace VisitFlowAPI.Models;

public enum UserRole
{
    Admin,
    HSE,
    RH,
    User
}

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Chemin public relatif (ex. /uploads/avatars/user_1_....jpg) servi depuis wwwroot.</summary>
    public string? AvatarUrl { get; set; }

    public ICollection<Intervention> CreatedInterventions { get; set; } = new List<Intervention>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<BlacklistRequest> ReviewedBlacklistRequests { get; set; } = new List<BlacklistRequest>();
}
