using FluentValidation;
using VisitFlowAPI.DTOs.Interventions;

namespace VisitFlowAPI.Application.Validation;

public class InterventionCreateDtoValidator : AbstractValidator<InterventionCreateDto>
{
    public InterventionCreateDtoValidator()
    {
        RuleFor(x => x.PlantId).GreaterThan(0).WithMessage("A plant must be selected.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.ZoneIds).NotEmpty().WithMessage("At least one zone is required.");
        RuleForEach(x => x.ZoneIds).GreaterThan(0);
        RuleFor(x => x.TypeOfWorkId).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
    }
}
