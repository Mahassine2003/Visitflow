using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Auth;
using VisitFlowAPI.Models;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly VisitFlowDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IAuthService _authService;

    public UsersController(VisitFlowDbContext db, IWebHostEnvironment env, IAuthService authService)
    {
        _db = db;
        _env = env;
        _authService = authService;
    }

    public class UserMiniDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    [HttpGet("rh")]
    public async Task<ActionResult<IEnumerable<UserMiniDto>>> GetRhUsers()
    {
        var rows = await _db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.RH)
            .OrderBy(u => u.FullName)
            .Select(u => new UserMiniDto { Id = u.Id, FullName = u.FullName })
            .ToListAsync();

        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        return Ok(new
        {
            user.Id,
            FullName = user.FullName,
            user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>Mise à jour du nom et de l’e-mail (utilisateur connecté = même id que la route).</summary>
    [HttpPut("{id:int}")]
    [HttpPatch("{id:int}")]
    public Task<ActionResult<object>> UpdateProfilePutOrPatch(int id, [FromBody] UpdateProfileRequest body) =>
        UpdateProfileCoreAsync(id, body);

    /// <summary>Même logique en POST sur un sous-chemin (évite certains 405 / proxies qui filtrent PUT).</summary>
    [HttpPost("{id:int}/profile")]
    public Task<ActionResult<object>> UpdateProfilePost(int id, [FromBody] UpdateProfileRequest body) =>
        UpdateProfileCoreAsync(id, body);

    /// <summary>Changement de mot de passe (même contrat que POST /api/auth/change-password ; chemin aligné sur /profile).</summary>
    [HttpPost("{id:int}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var callerId = GetCallerUserId();
        if (callerId is null || callerId.Value != id)
            return Forbid();

        try
        {
            await _authService.ChangePasswordAsync(id, request);
            return Ok(new { message = "Password updated." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<ActionResult<object>> UpdateProfileCoreAsync(int id, UpdateProfileRequest body)
    {
        var callerId = GetCallerUserId();
        if (callerId is null || callerId.Value != id)
            return Forbid();

        if (string.IsNullOrWhiteSpace(body.FullName))
            return BadRequest(new { message = "Full name is required." });
        if (string.IsNullOrWhiteSpace(body.Email))
            return BadRequest(new { message = "Email is required." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        var emailTrim = body.Email.Trim();
        var emailTaken = await _db.Users.AnyAsync(u => u.Id != id && u.Email == emailTrim);
        if (emailTaken)
            return Conflict(new { message = "This email is already in use." });

        user.FullName = body.FullName.Trim();
        user.Email = emailTrim;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            FullName = user.FullName,
            user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        });
    }

    private int? GetCallerUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (sub != null && int.TryParse(sub, out var uid))
            return uid;
        return null;
    }

    [HttpPost("{id:int}/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<object>> UploadAvatar(int id, [FromForm] IFormFile file)
    {
        var callerId = GetCallerUserId();
        if (callerId is null || callerId.Value != id)
            return Forbid();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();
        if (file == null || file.Length == 0) return BadRequest("Aucun fichier fourni.");

        var root = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsRoot = Path.Combine(root, "uploads", "avatars");
        Directory.CreateDirectory(uploadsRoot);

        TryDeleteOldAvatarFile(root, user.AvatarUrl);

        var fileName = $"user_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(uploadsRoot, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var publicUrl = $"/uploads/avatars/{fileName}";
        user.AvatarUrl = publicUrl;
        await _db.SaveChangesAsync();

        return Ok(new { avatarUrl = publicUrl });
    }

    /// <summary>Supprime l’ancien fichier sous wwwroot si l’URL pointe vers uploads/avatars.</summary>
    private static void TryDeleteOldAvatarFile(string wwwroot, string? previousPublicUrl)
    {
        if (string.IsNullOrEmpty(previousPublicUrl)) return;
        if (!previousPublicUrl.StartsWith("/uploads/avatars/", StringComparison.Ordinal)) return;
        var relative = previousPublicUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var path = Path.Combine(wwwroot, relative);
        var full = Path.GetFullPath(path);
        var rootFull = Path.GetFullPath(wwwroot);
        if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)) return;
        try
        {
            if (System.IO.File.Exists(full))
                System.IO.File.Delete(full);
        }
        catch
        {
            /* ignore — remplacement prioritaire */
        }
    }
}
