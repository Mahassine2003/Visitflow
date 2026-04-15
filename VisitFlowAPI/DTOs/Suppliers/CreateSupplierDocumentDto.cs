namespace VisitFlowAPI.DTOs.Suppliers;

public class CreateSupplierDocumentDto
{
    public int SupplierId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}
