using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/blacklist-requests")]
[Authorize]
public class BlacklistRequestsController : ControllerBase
{
    private readonly VisitFlowDbContext _db;

    public BlacklistRequestsController(VisitFlowDbContext db) => _db = db;

    public class PersonnelMiniDto
    {
        public string FullName { get; set; } = string.Empty;
    }

    public class ReviewedByUserMiniDto
    {
        public string FullName { get; set; } = string.Empty;
    }

    public class BlacklistRequestRowDto
    {
        public int Id { get; set; }
        public int PersonnelId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public PersonnelMiniDto Personnel { get; set; } = null!;
        public ReviewedByUserMiniDto? ReviewedByUser { get; set; }
    }

    [HttpGet]
    [Authorize] // filtrage différent selon le rôle
    public async Task<ActionResult<IEnumerable<BlacklistRequestRowDto>>> Get([FromQuery] BlacklistStatus? status)
    {
        var q = _db.BlacklistRequests
            .AsNoTracking()
            .Include(b => b.Personnel)
            .Include(b => b.ReviewedByUser)
            .AsQueryable();

        if (status.HasValue) q = q.Where(b => b.Status == status.Value);

        // Cas utilisateur "simple" : il voit uniquement ses propres demandes (historique personnel)
        if (User.IsInRole("USER") && !User.IsInRole("ADMIN") && !User.IsInRole("RH"))
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                q = q.Where(b => b.RequestedBy == userName);
            }
        }
        // Cas RH : plus de filtre → tous les RH voient toutes les demandes.

        var rows = await q
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BlacklistRequestRowDto
            {
                Id = b.Id,
                PersonnelId = b.PersonnelId,
                Reason = b.Reason,
                Status = b.Status.ToString(),
                RequestedBy = b.RequestedBy,
                CreatedAt = b.CreatedAt,
                Personnel = new PersonnelMiniDto { FullName = b.Personnel.FullName },
                ReviewedByUser = b.ReviewedByUser == null
                    ? null
                    : new ReviewedByUserMiniDto { FullName = b.ReviewedByUser.FullName }
            })
            .ToListAsync();

        return Ok(rows);
    }

    public class CreateBlacklistRequestDto
    {
        public int PersonnelId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int? ReviewerUserId { get; set; }
    }

    [HttpPost]
    [Authorize(Roles = "USER,ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateBlacklistRequestDto dto)
    {
        var user = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "user";

        if (dto.ReviewerUserId.HasValue)
        {
            var reviewer = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.ReviewerUserId.Value);
            if (reviewer is null) return BadRequest("RH introuvable.");
            if (reviewer.Role != UserRole.RH) return BadRequest("Le compte sélectionné n'est pas un RH.");
        }

        var row = new BlacklistRequest
        {
            PersonnelId = dto.PersonnelId,
            Reason = dto.Reason,
            Status = BlacklistStatus.Pending,
            RequestedBy = user,
            ReviewedByUserId = dto.ReviewerUserId,
            CreatedAt = DateTime.UtcNow
        };
        _db.BlacklistRequests.Add(row);
        await _db.SaveChangesAsync();

        // Pas besoin de renvoyer l'entité complète (avec les navigations),
        // le frontend recharge la liste juste après.
        return NoContent();
    }

    [HttpPost("{id:int}/review")]
    [Authorize(Roles = "ADMIN,RH")]
    public async Task<IActionResult> Review(int id, [FromQuery] bool approve)
    {
        var row = await _db.BlacklistRequests.Include(b => b.Personnel).FirstOrDefaultAsync(b => b.Id == id);
        if (row is null) return NotFound();

        var reviewerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reviewerId = int.TryParse(reviewerIdClaim, out var rid) ? rid : (int?)null;

        if (reviewerId is null) return Unauthorized();
        if (row.ReviewedByUserId.HasValue && row.ReviewedByUserId.Value != reviewerId.Value)
            return Forbid();

        row.Status = approve ? BlacklistStatus.Approved : BlacklistStatus.Rejected;
        row.ReviewedByUserId = reviewerId.Value;
        if (approve) row.Personnel.IsBlacklisted = true;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
