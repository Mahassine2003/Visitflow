using Microsoft.AspNetCore.Http;
using VisitFlowAPI.DTOs.Ai;

namespace VisitFlowAPI.Services.Interfaces;

public interface IAiService
{
    Task ProcessOcrResultAsync(string ocrJson);

    Task<InsuranceValidationResultDto> ValidateInsuranceAsync(IFormFile file);

    /// <summary>
    /// Valide par IA tout document uploadé (assurance, pièces jointes, etc.), sauf les fichiers PDF.
    /// </summary>
    Task<DocumentValidationResultDto> ValidateDocumentAsync(IFormFile file);
}
