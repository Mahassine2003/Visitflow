using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitFlowAPI.DTOs.Admin;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("type-of-works")]
    public async Task<ActionResult<IEnumerable<TypeOfWorkDto>>> GetTypeOfWorks()
    {
        var result = await _adminService.GetTypeOfWorksAsync();
        return Ok(result);
    }

    [HttpPost("type-of-works")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<TypeOfWorkDto>> CreateTypeOfWork([FromBody] TypeOfWorkDto dto)
    {
        var created = await _adminService.CreateTypeOfWorkAsync(dto);
        return CreatedAtAction(nameof(GetTypeOfWorks), new { id = created.Id }, created);
    }

    [HttpPost("type-of-works/{typeOfWorkId:int}/requirements")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ComplianceRequirementDto>> CreateTypeOfWorkRequirement(
        int typeOfWorkId,
        [FromBody] CreateComplianceRequirementDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Le titre est requis.");

        var created = await _adminService.CreateRequirementForTypeOfWorkAsync(typeOfWorkId, dto);
        if (created is null) return NotFound();
        return CreatedAtAction(nameof(GetTypeOfWorks), new { }, created);
    }

    [HttpDelete("type-of-works/requirements/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteTypeOfWorkRequirement(int id)
    {
        var ok = await _adminService.DeleteTypeOfWorkRequirementAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpGet("zones")]
    public async Task<ActionResult<IEnumerable<ZoneDto>>> GetZones()
    {
        var result = await _adminService.GetZonesAsync();
        return Ok(result);
    }

    [HttpPost("zones")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ZoneDto>> CreateZone([FromBody] ZoneDto dto)
    {
        var created = await _adminService.CreateZoneAsync(dto);
        return CreatedAtAction(nameof(GetZones), new { id = created.Id }, created);
    }

    [HttpPut("zones/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ZoneDto>> UpdateZone(int id, [FromBody] ZoneDto dto)
    {
        var updated = await _adminService.UpdateZoneAsync(id, dto);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("zones/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteZone(int id)
    {
        var result = await _adminService.DeleteZoneAsync(id);
        if (result is null) return NotFound();
        if (result == false)
            return Conflict("Cette zone est utilisée par des interventions et ne peut pas être supprimée.");
        return NoContent();
    }

    [HttpPut("type-of-works/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<TypeOfWorkDto>> UpdateTypeOfWork(int id, [FromBody] TypeOfWorkDto dto)
    {
        var updated = await _adminService.UpdateTypeOfWorkAsync(id, dto);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("type-of-works/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteTypeOfWork(int id)
    {
        var result = await _adminService.DeleteTypeOfWorkAsync(id);
        if (result is null) return NotFound();
        if (result == false)
            return Conflict("Ce type de travail est utilisé par des interventions ou du personnel et ne peut pas être supprimé.");
        return NoContent();
    }

    [HttpPost("personnel/{personnelId:int}/blacklist")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> BlacklistPersonnel(int personnelId, [FromQuery] bool isBlacklisted = true)
    {
        await _adminService.BlacklistPersonnelAsync(personnelId, isBlacklisted);
        return NoContent();
    }
}

