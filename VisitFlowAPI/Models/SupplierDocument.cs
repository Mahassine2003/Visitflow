namespace VisitFlowAPI.Models;

/// <summary>Fichiers rattachés au fournisseur (hors intervention).</summary>
public class SupplierDocument
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Supplier Supplier { get; set; } = null!;
}
