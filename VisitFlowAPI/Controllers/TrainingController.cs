using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingController : ControllerBase
{
    private readonly VisitFlowDbContext _db;
    public TrainingController(VisitFlowDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Training>>> Get() =>
        Ok(await _db.Trainings.ToListAsync());

    [HttpPost]
    [Authorize(Roles = "ADMIN,RH")]
    public async Task<ActionResult<Training>> Create([FromBody] Training model)
    {
        _db.Trainings.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPost("assign")]
    [Authorize(Roles = "ADMIN,RH")]
    public async Task<IActionResult> Assign([FromBody] PersonnelTraining model)
    {
        _db.PersonnelTrainings.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }
}
