namespace VisitFlowAPI.DTOs.Suppliers;

public class SupplierDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ICE { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Status { get; set; } = string.Empty;
}

