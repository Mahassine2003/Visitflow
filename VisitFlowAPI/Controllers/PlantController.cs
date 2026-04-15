using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Plants;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlantController : ControllerBase
{
    private readonly VisitFlowDbContext _db;

    public PlantController(VisitFlowDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlantDto>>> GetPlants()
    {
        var list = await _db.Plants
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PlantDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            })
            .ToListAsync();
        return Ok(list);
    }
}
