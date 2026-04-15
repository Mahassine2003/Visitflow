namespace VisitFlowAPI.DTOs.Auth;

public class LoginRequest
{
    // Login principal par email
    public string? Email { get; set; }

    // Optionnel : permet aussi de garder la compatibilité si on veut se loguer par username
    public string? Username { get; set; }

    public string Password { get; set; } = string.Empty;
}

