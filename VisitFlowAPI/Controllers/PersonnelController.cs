using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using VisitFlowAPI.Data;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PersonnelController : ControllerBase
{
    private readonly VisitFlowDbContext _db;
    public PersonnelController(VisitFlowDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Personnel>>> Get([FromQuery] int? supplierId)
    {
        var q = _db.Personnels
            .AsNoTracking()
            .Include(p => p.TypeOfWork)
            .AsQueryable();

        if (supplierId.HasValue) q = q.Where(x => x.SupplierId == supplierId.Value);

        var list = await q
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.FullName,
                p.Cin,
                p.Phone,
                Position = p.Position,
                p.JobTitle,
                p.Address,
                p.StartDate,
                p.EndDate,
                p.SupplierId,
                p.TypeOfWorkId,
                TypeOfWorkName = p.TypeOfWork != null ? p.TypeOfWork.Name : null,
                p.IsBlacklisted,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Personnel>> GetById(int id)
    {
        var p = await _db.Personnels
            .AsNoTracking()
            .Include(x => x.TypeOfWork)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();

        return Ok(new
        {
            p.Id,
            p.FullName,
            p.Cin,
            p.Phone,
            Position = p.Position,
            p.JobTitle,
            p.Address,
            p.StartDate,
            p.EndDate,
            p.SupplierId,
            p.TypeOfWorkId,
            TypeOfWorkName = p.TypeOfWork != null ? p.TypeOfWork.Name : null,
            p.IsBlacklisted,
            p.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,RH,USER")]
    public async Task<ActionResult<Personnel>> Create([FromBody] Personnel model)
    {
        var supplierExists = await _db.Suppliers.AnyAsync(s => s.Id == model.SupplierId);
        if (!supplierExists)
        {
            return BadRequest(new { message = "Supplier not found for this personnel." });
        }

        _db.Personnels.Add(model);
        await _db.SaveChangesAsync();
        return Ok(model);
    }

    [HttpGet("{personnelId:int}/documents")]
    public async Task<ActionResult<IEnumerable<object>>> GetDocuments(int personnelId)
    {
        var list = await _db.ComplianceItems
            .AsNoTracking()
            .Where(x => x.PersonnelId == personnelId)
            .OrderByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                PersonnelId = x.PersonnelId ?? 0,
                DocumentType = x.Type,
                FilePath = x.TitleOrFilePath,
                x.IsValid,
                x.ValidatedByAI
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("{personnelId:int}/documents/upload")]
    [Authorize(Roles = "ADMIN,RH,USER")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<object>> UploadDocument(
        int personnelId,
        [FromForm] string documentType,
        [FromForm] bool isValid,
        [FromForm] bool validatedByAI,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier envoyé.");

        var personnelExists = await _db.Personnels.AnyAsync(p => p.Id == personnelId);
        if (!personnelExists) return BadRequest("Personnel invalide.");

        var baseDir = Path.Combine("C:\\VisitFlow\\Uploads", "Personnels", personnelId.ToString());
        Directory.CreateDirectory(baseDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(baseDir, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var item = new ComplianceItem
        {
            PersonnelId = personnelId,
            Type = string.IsNullOrWhiteSpace(documentType) ? "Document" : documentType.Trim(),
            TitleOrFilePath = fullPath,
            IsValid = isValid,
            ValidatedByAI = validatedByAI
        };

        _db.ComplianceItems.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            item.Id,
            PersonnelId = item.PersonnelId ?? 0,
            DocumentType = item.Type,
            FilePath = item.TitleOrFilePath,
            item.IsValid,
            item.ValidatedByAI
        });
    }

    [HttpGet("documents/{id:int}/download")]
    public IActionResult DownloadDocument(int id)
    {
        var doc = _db.ComplianceItems
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id && x.PersonnelId != null);

        if (doc is null) return NotFound();

        var path = doc.TitleOrFilePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return NotFound("File not found on server.");

        var ext = Path.GetExtension(path).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => MediaTypeNames.Application.Pdf,
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => MediaTypeNames.Application.Octet
        };

        // Sans nom de fichier dans PhysicalFile : pas de Content-Disposition: attachment,
        // ce qui permet au navigateur d’afficher le PDF / l’image (view) au lieu de forcer le téléchargement.
        return PhysicalFile(path, contentType, enableRangeProcessing: true);
    }

    [HttpPost("{id:int}/blacklist")]
    [Authorize(Roles = "ADMIN,RH")]
    public async Task<IActionResult> Blacklist(int id, [FromQuery] bool isBlacklisted = true)
    {
        var p = await _db.Personnels.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        p.IsBlacklisted = isBlacklisted;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN,RH")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Personnels.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        _db.Personnels.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
