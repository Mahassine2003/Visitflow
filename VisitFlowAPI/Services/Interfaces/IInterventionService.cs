using VisitFlowAPI.DTOs.Interventions;

namespace VisitFlowAPI.Services.Interfaces;

public interface IInterventionService
{
    Task<InterventionDto> CreateInterventionAsync(InterventionCreateDto dto, string createdBy, int userId);
    Task<IEnumerable<InterventionDto>> GetInterventionsAsync();
    Task<InterventionDetailDto?> GetInterventionDetailAsync(int id);
    Task AssignPersonnelAsync(int interventionId, IEnumerable<int> personnelIds);
    Task ApproveHseAsync(int interventionId, bool approved, string? approverName = null);
    Task<InterventionElementDto> AddInterventionElementAsync(int interventionId, AddInterventionElementDto dto);
    Task<InterventionElementDto> UpdateInterventionElementFieldsAsync(int interventionId, int elementId, UpdateInterventionElementFieldsDto dto);
    Task<SafetyMeasureDto> AddSafetyMeasureAsync(int interventionId, string description, string addedBy);
    Task UpdateWorkflowAsync(int interventionId, UpdateInterventionWorkflowDto dto);
}
