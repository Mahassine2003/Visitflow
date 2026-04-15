namespace VisitFlowAPI.DTOs.Suppliers;

public class SupplierDocumentDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
