using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitFlowAPI.DTOs.Admin;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/type-of-work")]
[Authorize]
public class TypeOfWorkController : ControllerBase
{
    private readonly IAdminService _adminService;

    public TypeOfWorkController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Catalogue des types de travail (tous rôles authentifiés).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TypeOfWorkDto>>> GetAll()
    {
        var result = await _adminService.GetTypeOfWorksAsync();
        return Ok(result);
    }

    [HttpGet("{typeOfWorkId:int}/requirements")]
    public async Task<ActionResult<IEnumerable<ComplianceRequirementDto>>> GetRequirements(int typeOfWorkId)
    {
        var result = await _adminService.GetRequirementsForTypeOfWorkAsync(typeOfWorkId);
        return Ok(result);
    }
}
