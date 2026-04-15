namespace VisitFlowAPI.Models;

public class TypeOfWork
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresInsurance { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TypeOfWorkInsurance> TypeOfWorkInsurances { get; set; } = new List<TypeOfWorkInsurance>();
    public ICollection<TypeOfWorkTraining> TypeOfWorkTrainings { get; set; } = new List<TypeOfWorkTraining>();
    public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    public ICollection<ComplianceItem> ComplianceItems { get; set; } = new List<ComplianceItem>();
    public ICollection<Personnel> Personnels { get; set; } = new List<Personnel>();
}
