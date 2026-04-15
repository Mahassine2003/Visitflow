using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ValidationController : ControllerBase
{
    private readonly VisitFlowDbContext _db;
    public ValidationController(VisitFlowDbContext db) => _db = db;

    [HttpPost("{interventionId:int}/approve")]
    [Authorize(Roles = "ADMIN,HSE")]
    public async Task<IActionResult> Approve(int interventionId, [FromBody] string? comment = null)
    {
        return await ValidateInternal(interventionId, true, comment);
    }

    [HttpPost("{interventionId:int}/reject")]
    [Authorize(Roles = "ADMIN,HSE")]
    public async Task<IActionResult> Reject(int interventionId, [FromBody] string? comment = null)
    {
        return await ValidateInternal(interventionId, false, comment);
    }

    private async Task<IActionResult> ValidateInternal(int interventionId, bool approved, string? comment)
    {
        var intervention = await _db.Interventions
            .Include(i => i.TypeOfWork)
            .Include(i => i.InterventionPersonnels)
            .FirstOrDefaultAsync(i => i.Id == interventionId);
        if (intervention is null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var assignedIds = intervention.InterventionPersonnels.Select(x => x.PersonnelId).ToList();
        var hasAssignedPersonnel = assignedIds.Count > 0;

        // Business rule: insurance is always mandatory for type of work.
        var hasValidInsurance = hasAssignedPersonnel && await _db.Insurances.AnyAsync(x =>
            assignedIds.Contains(x.PersonnelId) && x.IsValid && x.ExpiryDate >= today);

        var hasBlacklisted = await _db.Personnels.AnyAsync(x => assignedIds.Contains(x.Id) && x.IsBlacklisted);

        // Business rule: training is optional (no blocking validation by training).
        if (approved && (!hasAssignedPersonnel || !hasValidInsurance || hasBlacklisted))
        {
            return BadRequest(new
            {
                message = "Intervention cannot be approved. Check assigned personnel, valid insurance and blacklist constraints.",
                hasAssignedPersonnel,
                hasValidInsurance,
                hasBlacklistedPersonnel = hasBlacklisted
            });
        }

        intervention.IsHSEValidated = approved;
        intervention.HSEComment = comment ?? string.Empty;
        intervention.Status = approved ? InterventionStatus.Validated : InterventionStatus.Rejected;
        await _db.SaveChangesAsync();
        return Ok(new
        {
            intervention.Id,
            intervention.Status,
            intervention.IsHSEValidated,
            intervention.HSEComment
        });
    }
}
