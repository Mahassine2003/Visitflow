namespace VisitFlowAPI.Models;

public class Personnel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Cin { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsBlacklisted { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int SupplierId { get; set; }
    public int? TypeOfWorkId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Supplier Supplier { get; set; } = null!;
    public TypeOfWork? TypeOfWork { get; set; }
    public ICollection<Insurance> Insurances { get; set; } = new List<Insurance>();
    public ICollection<PersonnelTraining> PersonnelTrainings { get; set; } = new List<PersonnelTraining>();
    public ICollection<InterventionPersonnel> InterventionPersonnels { get; set; } = new List<InterventionPersonnel>();
    public ICollection<ComplianceItem> ComplianceItems { get; set; } = new List<ComplianceItem>();
    public ICollection<BlacklistRequest> BlacklistRequests { get; set; } = new List<BlacklistRequest>();
}
