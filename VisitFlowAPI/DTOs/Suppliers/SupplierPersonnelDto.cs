namespace VisitFlowAPI.DTOs.Suppliers;

public class SupplierPersonnelDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string FieldOfActivity { get; set; } = string.Empty;
    public bool IsBlacklisted { get; set; }
    public int? TypeOfWorkId { get; set; }
    public string? TypeOfWorkName { get; set; }
}

