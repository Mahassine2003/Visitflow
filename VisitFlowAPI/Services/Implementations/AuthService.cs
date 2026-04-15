using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VisitFlowAPI.DTOs.Auth;
using VisitFlowAPI.Models;
using VisitFlowAPI.Repositories;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingByEmail = (await _unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
        if (existingByEmail is not null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = CreatePasswordHash(request.Password),
            Role = ParseRole(request.Role)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return GenerateTokens(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var userQuery = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);

        var user = userQuery.FirstOrDefault();
        if (user is null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        if (!VerifyPasswordHash(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        return GenerateTokens(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (!VerifyPasswordHash(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("New password must be at least 8 characters.");
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            throw new InvalidOperationException("New password must be different from the current password.");
        }

        user.PasswordHash = CreatePasswordHash(request.NewPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private AuthResponse GenerateTokens(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString().ToUpperInvariant())
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiresInMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(token);

        // Pour l'instant, on génère juste un GUID comme refresh token en mémoire.
        var refreshToken = Guid.NewGuid().ToString("N");

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expires,
            Username = user.Email,
            Role = user.Role.ToString().ToUpperInvariant()
        };
    }

    private static string CreatePasswordHash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPasswordHash(string password, string storedHash)
    {
        var computed = CreatePasswordHash(password);
        return string.Equals(computed, storedHash, StringComparison.Ordinal);
    }

    private static UserRole ParseRole(string role)
    {
        if (string.Equals(role, "SUPPLIER", StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.User;
        }

        if (Enum.TryParse<UserRole>(role, true, out var parsed))
        {
            return parsed;
        }

        return UserRole.User;
    }
}

