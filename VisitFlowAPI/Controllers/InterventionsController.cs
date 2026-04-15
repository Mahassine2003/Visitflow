using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Common;
using VisitFlowAPI.DTOs.Interventions;
using VisitFlowAPI.Models;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterventionsController : ControllerBase
{
    private readonly VisitFlowDbContext _db;
    private readonly IInterventionService _interventionService;

    public InterventionsController(VisitFlowDbContext db, IInterventionService interventionService)
    {
        _db = db;
        _interventionService = interventionService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Intervention>>> Get([FromQuery] QueryParams query, [FromQuery] string? status)
    {
        var q = _db.Interventions.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InterventionStatus>(status, true, out var parsedStatus))
        {
            q = q.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            q = q.Where(x => x.Title.Contains(query.Search) || x.Description.Contains(query.Search));
        }

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        return Ok(new PagedResult<Intervention> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize });
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<InterventionDto>> Create([FromBody] InterventionCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var createdByUserId = int.TryParse(userIdClaim, out var uid) ? uid : 1;
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "system";
        try
        {
            var created = await _interventionService.CreateInterventionAsync(dto, username, createdByUserId);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/assign-personnel")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<IActionResult> AssignPersonnel(int id, [FromBody] IEnumerable<int> personnelIds)
    {
        await _interventionService.AssignPersonnelAsync(id, personnelIds);
        return NoContent();
    }
}
