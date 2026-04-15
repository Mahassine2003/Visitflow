namespace VisitFlowAPI.Models;

public class ComplianceItem
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TitleOrFilePath { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public bool ValidatedByAI { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PersonnelId { get; set; }
    public int? TypeOfWorkId { get; set; }

    public Personnel? Personnel { get; set; }
    public TypeOfWork? TypeOfWork { get; set; }
}
