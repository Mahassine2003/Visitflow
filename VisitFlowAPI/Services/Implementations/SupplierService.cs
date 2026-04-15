using VisitFlowAPI.DTOs.Documents;
using VisitFlowAPI.DTOs.Suppliers;
using VisitFlowAPI.Models;
using VisitFlowAPI.Repositories;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Services.Implementations;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SupplierDto>> GetSuppliersAsync()
    {
        var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
        return suppliers.Select(s => new SupplierDto
        {
            Id = s.Id,
            CompanyName = s.CompanyName,
            ICE = s.ComponentId,
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address,
            Status = string.Empty
        });
    }

    public async Task<SupplierDto> CreateSupplierAsync(SupplierCreateDto dto)
    {
        var entity = new Supplier
        {
            CompanyName = dto.CompanyName,
            ComponentId = dto.ICE ?? string.Empty,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address ?? string.Empty
        };

        await _unitOfWork.Suppliers.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return new SupplierDto
        {
            Id = entity.Id,
            CompanyName = entity.CompanyName,
            ICE = entity.ComponentId,
            Email = entity.Email,
            Phone = entity.Phone,
            Address = entity.Address,
            Status = string.Empty
        };
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
    {
        var s = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (s is null) return null;

        return new SupplierDto
        {
            Id = s.Id,
            CompanyName = s.CompanyName,
            ICE = s.ComponentId,
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address,
            Status = string.Empty
        };
    }

    public async Task<SupplierDto?> UpdateSupplierAsync(int id, SupplierUpdateDto dto)
    {
        var s = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (s is null) return null;

        s.CompanyName = dto.CompanyName.Trim();
        s.ComponentId = (dto.ICE ?? string.Empty).Trim();
        s.Email = dto.Email.Trim();
        s.Phone = dto.Phone.Trim();
        s.Address = (dto.Address ?? string.Empty).Trim();

        _unitOfWork.Suppliers.Update(s);
        await _unitOfWork.SaveChangesAsync();

        return new SupplierDto
        {
            Id = s.Id,
            CompanyName = s.CompanyName,
            ICE = s.ComponentId,
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address,
            Status = string.Empty
        };
    }

    public async Task<(bool Deleted, string? Error)> DeleteSupplierAsync(int id)
    {
        var s = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (s is null) return (false, "Supplier not found.");

        var hasInterventions = (await _unitOfWork.Interventions.FindAsync(i => i.SupplierId == id)).Any();
        if (hasInterventions)
            return (false, "Cannot delete: this supplier has interventions. Remove or reassign them first.");

        _unitOfWork.Suppliers.Remove(s);
        await _unitOfWork.SaveChangesAsync();
        return (true, null);
    }

    public async Task<IEnumerable<SupplierPersonnelDto>> GetPersonnelBySupplierAsync(int supplierId)
    {
        var personnel = await _unitOfWork.Personnels.FindAsync(p => p.SupplierId == supplierId);
        var typeOfWorks = (await _unitOfWork.TypeOfWorks.GetAllAsync()).ToDictionary(t => t.Id);

        return personnel.Select(p => new SupplierPersonnelDto
        {
            Id = p.Id,
            FullName = p.FullName,
            CIN = p.Cin,
            Phone = p.Phone,
            SupplierId = p.SupplierId,
            FieldOfActivity = p.Position,
            IsBlacklisted = p.IsBlacklisted,
            TypeOfWorkId = p.TypeOfWorkId,
            TypeOfWorkName = p.TypeOfWorkId is int tid && typeOfWorks.TryGetValue(tid, out var tw)
                ? tw.Name
                : null
        });
    }

    public async Task<SupplierPersonnelDto?> CreatePersonnelAsync(SupplierPersonnelDto dto)
    {
        if (dto.TypeOfWorkId is int tid)
        {
            if (await _unitOfWork.TypeOfWorks.GetByIdAsync(tid) is null)
                return null;
        }

        var entity = new Personnel
        {
            FullName = dto.FullName,
            Cin = dto.CIN,
            Phone = dto.Phone,
            SupplierId = dto.SupplierId,
            Position = dto.FieldOfActivity,
            Address = string.Empty,
            IsBlacklisted = dto.IsBlacklisted,
            TypeOfWorkId = dto.TypeOfWorkId
        };

        await _unitOfWork.Personnels.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = entity.Id;
        var tow = entity.TypeOfWorkId is int towId
            ? await _unitOfWork.TypeOfWorks.GetByIdAsync(towId)
            : null;
        dto.TypeOfWorkName = tow?.Name;
        return dto;
    }

    public async Task<DocumentDto> AddDocumentAsync(DocumentDto dto)
    {
        var entity = new Document
        {
            Name = dto.DocumentType,
            FilePath = dto.FilePath,
            FileType = dto.EntityType,
            InterventionId = dto.EntityId
        };

        await _unitOfWork.Documents.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<IEnumerable<SupplierDocumentDto>> GetSupplierDocumentsAsync(int supplierId)
    {
        if (await _unitOfWork.Suppliers.GetByIdAsync(supplierId) is null)
            return Array.Empty<SupplierDocumentDto>();

        var rows = await _unitOfWork.SupplierDocuments.FindAsync(d => d.SupplierId == supplierId);
        return rows
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new SupplierDocumentDto
            {
                Id = d.Id,
                SupplierId = d.SupplierId,
                DocumentType = d.DocumentType,
                FilePath = d.FilePath,
                FileType = d.FileType,
                UploadedAt = d.UploadedAt
            });
    }

    public async Task<SupplierDocumentDto?> AddSupplierDocumentAsync(CreateSupplierDocumentDto dto)
    {
        if (await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId) is null)
            return null;

        var type = (dto.DocumentType ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(type))
            return null;

        var entity = new SupplierDocument
        {
            SupplierId = dto.SupplierId,
            DocumentType = type,
            FilePath = dto.FilePath ?? string.Empty,
            FileType = dto.FileType ?? string.Empty
        };

        await _unitOfWork.SupplierDocuments.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return new SupplierDocumentDto
        {
            Id = entity.Id,
            SupplierId = entity.SupplierId,
            DocumentType = entity.DocumentType,
            FilePath = entity.FilePath,
            FileType = entity.FileType,
            UploadedAt = entity.UploadedAt
        };
    }

    public async Task<SupplierDocumentDto?> GetSupplierDocumentByIdAsync(int id)
    {
        var d = await _unitOfWork.SupplierDocuments.GetByIdAsync(id);
        if (d is null) return null;

        return new SupplierDocumentDto
        {
            Id = d.Id,
            SupplierId = d.SupplierId,
            DocumentType = d.DocumentType,
            FilePath = d.FilePath,
            FileType = d.FileType,
            UploadedAt = d.UploadedAt
        };
    }

    public async Task UpdateSupplierStatusAsync(int id, string status)
    {
        _ = status;
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Supplier not found");
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();
    }
}

