namespace VisitFlowAPI.Services.Interfaces;

public interface IPdfService
{
    Task<string> GenerateInterventionPdfAsync(int interventionId);
    Task<string> GenerateBlacklistPdfAsync();
}

