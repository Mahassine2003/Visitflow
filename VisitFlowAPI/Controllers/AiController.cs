using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VisitFlowAPI.DTOs.Ai;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiService aiService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    // Endpoint appelé par le microservice Python avec le JSON OCR (optionnel)
    [HttpPost("ocr-callback")]
    public async Task<IActionResult> OcrCallback([FromBody] object payload)
    {
        await _aiService.ProcessOcrResultAsync(payload.ToString() ?? string.Empty);
        return Ok();
    }

    // Endpoint pour que le front envoie directement l'image d'assurance
    [HttpPost("validate-insurance")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult<InsuranceValidationResultDto>> ValidateInsurance(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Aucun fichier envoyé." });
        }

        try
        {
            var result = await _aiService.ValidateInsuranceAsync(file);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Microservice IA injoignable");
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Service IA indisponible. Démarrez le microservice Python et vérifiez Ai:ServiceBaseUrl (ex. http://localhost:8000).",
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout appel microservice IA");
            return StatusCode(StatusCodes.Status504GatewayTimeout, new
            {
                message = "Le service IA n'a pas répondu à temps.",
            });
        }
    }

    /// <summary>
    /// Validation IA pour tout document intégré (sauf PDF). Les PDF renvoient AiSkipped=true sans appeler le service OCR.
    /// </summary>
    [HttpPost("validate-document")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<DocumentValidationResultDto>> ValidateDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Aucun fichier envoyé." });
        }

        try
        {
            var result = await _aiService.ValidateDocumentAsync(file);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Microservice IA injoignable");
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Service IA indisponible. Démarrez le microservice Python et vérifiez Ai:ServiceBaseUrl (ex. http://localhost:8000).",
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout appel microservice IA");
            return StatusCode(StatusCodes.Status504GatewayTimeout, new
            {
                message = "Le service IA n'a pas répondu à temps.",
            });
        }
    }
}

