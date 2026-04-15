namespace VisitFlowAPI.Models;

public class Supplier
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ComponentId { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Personnel> Personnel { get; set; } = new List<Personnel>();
    public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    public ICollection<SupplierDocument> SupplierDocuments { get; set; } = new List<SupplierDocument>();
}
