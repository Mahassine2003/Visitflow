using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/element-types")]
[Authorize]
public class ElementTypesController : ControllerBase
{
    private readonly VisitFlowDbContext _db;

    public ElementTypesController(VisitFlowDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> Get()
    {
        var items = await _db.ElementTypes
            .AsNoTracking()
            .Include(t => t.ElementOptions)
            .OrderBy(t => t.Name)
            .ToListAsync();
        return Ok(items.Select(t => new
        {
            t.Id,
            t.Name,
            options = t.ElementOptions.OrderBy(o => o.Id).Select(o => new { id = o.Id, label = o.Label })
        }));
    }
}
