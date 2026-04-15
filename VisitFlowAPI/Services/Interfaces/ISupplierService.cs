using VisitFlowAPI.DTOs.Documents;
using VisitFlowAPI.DTOs.Suppliers;

namespace VisitFlowAPI.Services.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetSuppliersAsync();
    Task<SupplierDto> CreateSupplierAsync(SupplierCreateDto dto);
    Task<SupplierDto?> GetSupplierByIdAsync(int id);
    Task<SupplierDto?> UpdateSupplierAsync(int id, SupplierUpdateDto dto);
    Task<(bool Deleted, string? Error)> DeleteSupplierAsync(int id);
    Task<IEnumerable<SupplierPersonnelDto>> GetPersonnelBySupplierAsync(int supplierId);
    Task<SupplierPersonnelDto?> CreatePersonnelAsync(SupplierPersonnelDto dto);
    Task<DocumentDto> AddDocumentAsync(DocumentDto dto);
    Task<IEnumerable<SupplierDocumentDto>> GetSupplierDocumentsAsync(int supplierId);
    Task<SupplierDocumentDto?> AddSupplierDocumentAsync(CreateSupplierDocumentDto dto);
    Task<SupplierDocumentDto?> GetSupplierDocumentByIdAsync(int id);
    Task UpdateSupplierStatusAsync(int id, string status);
}

