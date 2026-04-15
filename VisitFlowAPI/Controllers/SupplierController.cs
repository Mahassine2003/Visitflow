using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using VisitFlowAPI.DTOs.Documents;
using VisitFlowAPI.DTOs.Suppliers;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
    {
        var suppliers = await _supplierService.GetSuppliersAsync();
        return Ok(suppliers);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] SupplierCreateDto dto)
    {
        var created = await _supplierService.CreateSupplierAsync(dto);
        return CreatedAtAction(nameof(GetSupplierById), new { id = created.Id }, created);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDto>> GetSupplierById(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier is null) return NotFound();
        return Ok(supplier);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(int id, [FromBody] SupplierUpdateDto dto)
    {
        var updated = await _supplierService.UpdateSupplierAsync(id, dto);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var (deleted, error) = await _supplierService.DeleteSupplierAsync(id);
        if (!deleted)
            return error is "Supplier not found." ? NotFound() : BadRequest(new { message = error });
        return NoContent();
    }

    [HttpGet("{supplierId:int}/personnel")]
    public async Task<ActionResult<IEnumerable<SupplierPersonnelDto>>> GetPersonnel(int supplierId)
    {
        var list = await _supplierService.GetPersonnelBySupplierAsync(supplierId);
        return Ok(list);
    }

    [HttpPost("personnel")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<SupplierPersonnelDto>> CreatePersonnel([FromBody] SupplierPersonnelDto dto)
    {
        var created = await _supplierService.CreatePersonnelAsync(dto);
        if (created is null) return BadRequest("Type de travail invalide.");
        return CreatedAtAction(nameof(GetPersonnel), new { supplierId = created.SupplierId }, created);
    }

    [HttpPost("documents")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<DocumentDto>> AddDocument([FromBody] DocumentDto dto)
    {
        var created = await _supplierService.AddDocumentAsync(dto);
        return Created(string.Empty, created);
    }

    [HttpGet("{supplierId:int}/company-documents")]
    public async Task<ActionResult<IEnumerable<SupplierDocumentDto>>> GetCompanyDocuments(int supplierId)
    {
        var list = await _supplierService.GetSupplierDocumentsAsync(supplierId);
        return Ok(list);
    }

    [HttpPost("company-documents")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<ActionResult<SupplierDocumentDto>> AddCompanyDocument([FromBody] CreateSupplierDocumentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DocumentType))
            return BadRequest("Le type de document est requis.");

        var created = await _supplierService.AddSupplierDocumentAsync(dto);
        if (created is null) return BadRequest("Fournisseur invalide.");
        return Created(string.Empty, created);
    }

    [HttpPost("{supplierId:int}/company-documents/upload")]
    [Authorize(Roles = "ADMIN,USER")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<SupplierDocumentDto>> UploadCompanyDocument(
        int supplierId,
        [FromForm] string documentType,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier envoyé.");
        if (string.IsNullOrWhiteSpace(documentType))
            return BadRequest("Le type de document est requis.");

        var baseDir = Path.Combine("C:\\VisitFlow\\Uploads", "Suppliers", supplierId.ToString());
        Directory.CreateDirectory(baseDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(baseDir, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var dto = new CreateSupplierDocumentDto
        {
            SupplierId = supplierId,
            DocumentType = documentType.Trim(),
            FilePath = fullPath,
            FileType = file.ContentType ?? string.Empty
        };

        var created = await _supplierService.AddSupplierDocumentAsync(dto);
        if (created is null) return BadRequest("Fournisseur invalide.");
        return Created(string.Empty, created);
    }

    [HttpGet("company-documents/{id:int}/download")]
    public async Task<IActionResult> DownloadCompanyDocument(int id)
    {
        var doc = await _supplierService.GetSupplierDocumentByIdAsync(id);
        if (doc is null) return NotFound();

        var path = doc.FilePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            return NotFound("File not found on server.");

        var contentType = string.IsNullOrWhiteSpace(doc.FileType)
            ? MediaTypeNames.Application.Octet
            : doc.FileType;

        var fileName = Path.GetFileName(path);
        return PhysicalFile(path, contentType, fileName, enableRangeProcessing: true);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        await _supplierService.UpdateSupplierStatusAsync(id, status);
        return NoContent();
    }
}

