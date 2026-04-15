namespace VisitFlowAPI.Models;

public class Document
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int InterventionId { get; set; }

    public Intervention Intervention { get; set; } = null!;
    public ICollection<Form> Forms { get; set; } = new List<Form>();
}
