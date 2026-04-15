using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;

    public PdfController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpPost("interventions/{interventionId:int}")]
    public async Task<IActionResult> GenerateInterventionPdf(int interventionId)
    {
        var path = await _pdfService.GenerateInterventionPdfAsync(interventionId);
        return Ok(new { filePath = path });
    }

    [HttpGet("blacklist")]
    public async Task<IActionResult> GetBlacklistPdf()
    {
        var path = await _pdfService.GenerateBlacklistPdfAsync();
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var stream = System.IO.File.OpenRead(path);
        var fileName = System.IO.Path.GetFileName(path);
        return File(stream, "application/pdf", fileName);
    }

    [HttpGet("intervention/{interventionId:int}")]
    public async Task<IActionResult> GetInterventionPdf(int interventionId)
    {
        var path = await _pdfService.GenerateInterventionPdfAsync(interventionId);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var stream = System.IO.File.OpenRead(path);
        var fileName = System.IO.Path.GetFileName(path);
        return File(stream, "application/pdf", fileName);
    }
}

