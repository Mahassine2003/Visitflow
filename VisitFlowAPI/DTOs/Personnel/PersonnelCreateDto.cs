namespace VisitFlowAPI.DTOs.Personnel;

public class PersonnelCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public string Cin { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int SupplierId { get; set; }
}
