namespace VisitFlowAPI.Models;

public class TypeOfWorkInsurance
{
    public int TypeOfWorkId { get; set; }
    public int InsuranceId { get; set; }

    // Navigation
    public TypeOfWork TypeOfWork { get; set; } = null!;
    public Insurance Insurance { get; set; } = null!;
}
