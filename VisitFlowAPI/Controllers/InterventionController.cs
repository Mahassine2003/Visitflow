using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitFlowAPI.DTOs.Interventions;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterventionController : ControllerBase
{
    private readonly IInterventionService _interventionService;

    public InterventionController(IInterventionService interventionService)
    {
        _interventionService = interventionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterventionDto>>> GetInterventions()
    {
        var list = await _interventionService.GetInterventionsAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InterventionDetailDto>> GetIntervention(int id)
    {
        var d = await _interventionService.GetInterventionDetailAsync(id);
        return d is null ? NotFound() : Ok(d);
    }

    [HttpPost("{id:int}/elements")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<InterventionElementDto>> AddElement(int id, [FromBody] AddInterventionElementDto dto)
    {
        try
        {
            var created = await _interventionService.AddInterventionElementAsync(id, dto);
            return Ok(created);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:int}/elements/{elementId:int}/fields")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<InterventionElementDto>> UpdateElementFields(
        int id,
        int elementId,
        [FromBody] UpdateInterventionElementFieldsDto dto)
    {
        try
        {
            var updated = await _interventionService.UpdateInterventionElementFieldsAsync(id, elementId, dto);
            return Ok(updated);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/safety-measures")]
    [Authorize(Roles = "ADMIN,HSE")]
    public async Task<ActionResult<SafetyMeasureDto>> AddSafetyMeasure(int id, [FromBody] AddSafetyMeasureDto dto)
    {
        try
        {
            var name = User.FindFirstValue(ClaimTypes.Name) ?? "HSE";
            var created = await _interventionService.AddSafetyMeasureAsync(id, dto.Description, name);
            return Ok(created);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:int}/workflow")]
    [Authorize(Roles = "ADMIN,USER,HSE")]
    public async Task<IActionResult> UpdateWorkflow(int id, [FromBody] UpdateInterventionWorkflowDto dto)
    {
        try
        {
            await _interventionService.UpdateWorkflowAsync(id, dto);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<InterventionDto>> CreateIntervention([FromBody] InterventionCreateDto dto)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "system";
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.TryParse(userIdClaim, out var uid) ? uid : 1;
        try
        {
            var created = await _interventionService.CreateInterventionAsync(dto, username, userId);
            return CreatedAtAction(nameof(GetInterventions), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{interventionId:int}/assign-personnel")]
    public async Task<IActionResult> AssignPersonnel(int interventionId, [FromBody] IEnumerable<int> personnelIds)
    {
        await _interventionService.AssignPersonnelAsync(interventionId, personnelIds);
        return NoContent();
    }

    [HttpPost("{interventionId:int}/hse-approve")]
    [Authorize(Roles = "ADMIN,HSE")]
    public async Task<IActionResult> ApproveHse(int interventionId, [FromQuery] bool approved = true)
    {
        var approver = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        await _interventionService.ApproveHseAsync(interventionId, approved, approver);
        return NoContent();
    }
}
