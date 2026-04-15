using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using VisitFlowAPI.Data;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InsuranceController : ControllerBase
{
    private readonly VisitFlowDbContext _db;
    public InsuranceController(VisitFlowDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Insurance>>> Get([FromQuery] int? personnelId)
    {
        var q = _db.Insurances.AsQueryable();
        if (personnelId.HasValue) q = q.Where(x => x.PersonnelId == personnelId.Value);
        return Ok(await q.ToListAsync());
    }

    [HttpPost("{personnelId:int}/upload")]
    [Authorize(Roles = "ADMIN,USER")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<Insurance>> Upload(
        int personnelId,
        IFormFile file,
        [FromForm] bool isValid,
        [FromForm] string? issueDate,
        [FromForm] string? expiryDate,
        [FromForm] bool validatedByAi = true)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier envoyé.");

        var exists = await _db.Personnels.AsNoTracking().AnyAsync(p => p.Id == personnelId);
        if (!exists) return NotFound("Personnel not found.");

        var baseDir = Path.Combine("C:\\VisitFlow\\Uploads", "Insurances", personnelId.ToString());
        Directory.CreateDirectory(baseDir);
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(baseDir, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        static DateOnly ParseDateOrToday(string? s)
        {
            if (DateTime.TryParse(s, out var dt))
                return DateOnly.FromDateTime(dt);
            return DateOnly.FromDateTime(DateTime.UtcNow);
        }

        var entity = new Insurance
        {
            PersonnelId = personnelId,
            Name = "Insurance",
            FilePath = fullPath,
            Phone = string.Empty,
            IssueDate = ParseDateOrToday(issueDate),
            ExpiryDate = ParseDateOrToday(expiryDate),
            IsValid = isValid,
            ValidatedByAi = validatedByAi
        };

        _db.Insurances.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { personnelId }, entity);
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var insurance = await _db.Insurances.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (insurance is null) return NotFound();

        var path = insurance.FilePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return NotFound("File not found on server.");

        var ext = Path.GetExtension(path).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => MediaTypeNames.Application.Pdf,
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => MediaTypeNames.Application.Octet
        };

        return PhysicalFile(path, contentType, enableRangeProcessing: true);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<Insurance>> Create([FromBody] Insurance model)
    {
        var personnelExists = await _db.Personnels.AnyAsync(p => p.Id == model.PersonnelId);
        if (!personnelExists)
        {
            return BadRequest(new { message = "Personnel not found for this insurance." });
        }

        _db.Insurances.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPost("{id:int}/validate-ai")]
    [Authorize(Roles = "ADMIN,HSE,RH")]
    public async Task<IActionResult> ValidateByAi(int id)
    {
        var insurance = await _db.Insurances.FirstOrDefaultAsync(x => x.Id == id);
        if (insurance is null) return NotFound();

        // Simule un AI service de validation assurance.
        insurance.IsValid = insurance.ExpiryDate >= DateOnly.FromDateTime(DateTime.UtcNow);
        await _db.SaveChangesAsync();
        return Ok(new { insurance.Id, insurance.IsValid });
    }
}
