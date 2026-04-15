using VisitFlowAPI.DTOs.Admin;

namespace VisitFlowAPI.Services.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<TypeOfWorkDto>> GetTypeOfWorksAsync();
    Task<TypeOfWorkDto> CreateTypeOfWorkAsync(TypeOfWorkDto dto);
    Task<TypeOfWorkDto?> UpdateTypeOfWorkAsync(int id, TypeOfWorkDto dto);
    /// <summary>Null = introuvable, false = utilisé (intervention ou personnel), true = supprimé.</summary>
    Task<bool?> DeleteTypeOfWorkAsync(int id);
    Task<IEnumerable<ComplianceRequirementDto>> GetRequirementsForTypeOfWorkAsync(int typeOfWorkId);
    Task<ComplianceRequirementDto?> CreateRequirementForTypeOfWorkAsync(int typeOfWorkId, CreateComplianceRequirementDto dto);
    Task<bool> DeleteTypeOfWorkRequirementAsync(int requirementId);
    Task<IEnumerable<ZoneDto>> GetZonesAsync();
    Task<ZoneDto> CreateZoneAsync(ZoneDto dto);
    Task<ZoneDto?> UpdateZoneAsync(int id, ZoneDto dto);
    /// <summary>Null = introuvable, false = zone liée à des interventions, true = supprimée.</summary>
    Task<bool?> DeleteZoneAsync(int id);
    Task BlacklistPersonnelAsync(int personnelId, bool isBlacklisted);
}

