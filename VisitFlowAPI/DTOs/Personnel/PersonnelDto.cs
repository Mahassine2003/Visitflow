namespace VisitFlowAPI.DTOs.Personnel;

public class PersonnelDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Cin { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsBlacklisted { get; set; }
    public int SupplierId { get; set; }
    public DateTime CreatedAt { get; set; }
}
