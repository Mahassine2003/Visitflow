namespace VisitFlowAPI.Models;

public class SupplierPersonnel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string FieldOfActivity { get; set; } = string.Empty;
    public bool IsBlacklisted { get; set; }
    public bool IsDeleted { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<PersonnelTraining> PersonnelTrainings { get; set; } = new List<PersonnelTraining>();
}

