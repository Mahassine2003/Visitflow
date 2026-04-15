using AutoMapper;
using VisitFlowAPI.DTOs.Interventions;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Application.Mapping;

public class VisitFlowMappingProfile : Profile
{
    public VisitFlowMappingProfile()
    {
        CreateMap<Intervention, InterventionDto>()
            .ForMember(d => d.VisitKey, o => o.MapFrom(s => s.Visit != null ? s.Visit.VisitId : string.Empty))
            .ForMember(d => d.ZoneIds, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.HSEApprovalStatus, o => o.MapFrom(s => s.IsHSEValidated ? "Validated" : "Pending"))
            .ForMember(d => d.StartDate, o => o.MapFrom(s => DateOnly.FromDateTime(s.StartDate)))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => DateOnly.FromDateTime(s.EndDate)))
            .ForMember(d => d.StartTime, o => o.MapFrom(s => TimeOnly.FromDateTime(s.StartDate)))
            .ForMember(d => d.EndTime, o => o.MapFrom(s => TimeOnly.FromDateTime(s.EndDate)))
            .ForMember(d => d.IsHseValidated, o => o.MapFrom(s => s.IsHSEValidated));
    }
}
