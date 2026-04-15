namespace VisitFlowAPI.DTOs.Suppliers;

public class SupplierUpdateDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string ICE { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
}
