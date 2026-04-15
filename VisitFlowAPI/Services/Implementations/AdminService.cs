using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Admin;
using VisitFlowAPI.Models;
using VisitFlowAPI.Repositories;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VisitFlowDbContext _db;

    public AdminService(IUnitOfWork unitOfWork, VisitFlowDbContext db)
    {
        _unitOfWork = unitOfWork;
        _db = db;
    }

    public async Task<IEnumerable<TypeOfWorkDto>> GetTypeOfWorksAsync()
    {
        var items = await _unitOfWork.TypeOfWorks.GetAllAsync();
        var ids = items.Select(t => t.Id).ToList();

        // Pièces modèle (sans Personnel) : uniquement le type Document pour les types de travail.
        var trainingIds = await _db.ComplianceItems.AsNoTracking()
            .Where(c =>
                c.PersonnelId == null &&
                c.TypeOfWorkId != null &&
                ids.Contains(c.TypeOfWorkId.Value) &&
                c.Type == "Document")
            .Select(c => c.TypeOfWorkId!.Value)
            .Distinct()
            .ToListAsync();

        var trainingSet = trainingIds.ToHashSet();

        return items.Select(t => new TypeOfWorkDto
        {
            Id = t.Id,
            Name = t.Name,
            RequiresInsurance = t.RequiresInsurance,
            Description = t.Description,
            RequiresTraining = trainingSet.Contains(t.Id)
        });
    }

    public async Task<TypeOfWorkDto> CreateTypeOfWorkAsync(TypeOfWorkDto dto)
    {
        var entity = new TypeOfWork
        {
            Name = dto.Name,
            RequiresInsurance = true,
            Description = dto.Description ?? string.Empty
        };

        await _unitOfWork.TypeOfWorks.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.RequiresInsurance = true;

        dto.RequiresTraining = await _db.ComplianceItems.AsNoTracking().AnyAsync(c =>
            c.PersonnelId == null &&
            c.TypeOfWorkId == entity.Id &&
            c.Type == "Document");

        return dto;
    }

    public async Task<TypeOfWorkDto?> UpdateTypeOfWorkAsync(int id, TypeOfWorkDto dto)
    {
        var entity = await _unitOfWork.TypeOfWorks.GetByIdAsync(id);
        if (entity is null) return null;

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description ?? string.Empty;
        entity.RequiresInsurance = dto.RequiresInsurance;

        // Si RequiresTraining est null => on ne touche pas à la formation (updates partiels).
        var requiresTraining = dto.RequiresTraining;

        if (requiresTraining != null)
        {
            if (!requiresTraining.Value)
            {
                // Case formation décochée => suppression des exigences Document (et anciennes Training) modèle.
                var existing = await _unitOfWork.ComplianceItems.FindAsync(x =>
                    x.PersonnelId == null &&
                    x.TypeOfWorkId == id &&
                    (x.Type == "Training" || x.Type == "Document"));

                foreach (var item in existing)
                    _unitOfWork.ComplianceItems.Remove(item);
            }
        }

        _unitOfWork.TypeOfWorks.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        // Dans le retour, on renvoie la formation telle que voulue (si fournie),
        // sinon on calcule à partir de la base pour rester cohérent.
        if (requiresTraining != null)
            return new TypeOfWorkDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                RequiresInsurance = entity.RequiresInsurance,
                RequiresTraining = requiresTraining.Value
            };

        // fallback (cas requiresTraining == null)
        var trainingExists = await _db.ComplianceItems.AsNoTracking().AnyAsync(c =>
            c.PersonnelId == null &&
            c.TypeOfWorkId == id &&
            c.Type == "Document");

        return new TypeOfWorkDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            RequiresInsurance = entity.RequiresInsurance,
            RequiresTraining = trainingExists
        };
    }

    public async Task<bool?> DeleteTypeOfWorkAsync(int id)
    {
        var entity = await _unitOfWork.TypeOfWorks.GetByIdAsync(id);
        if (entity is null) return null;

        var usedByIntervention = (await _unitOfWork.Interventions.FindAsync(i => i.TypeOfWorkId == id)).Any();
        if (usedByIntervention) return false;

        var usedByPersonnel = (await _unitOfWork.Personnels.FindAsync(p => p.TypeOfWorkId == id)).Any();
        if (usedByPersonnel) return false;

        foreach (var c in await _unitOfWork.ComplianceItems.FindAsync(x => x.TypeOfWorkId == id))
        {
            _unitOfWork.ComplianceItems.Remove(c);
        }

        foreach (var ti in await _unitOfWork.TypeOfWorkInsurances.FindAsync(x => x.TypeOfWorkId == id))
            _unitOfWork.TypeOfWorkInsurances.Remove(ti);

        foreach (var tt in await _unitOfWork.TypeOfWorkTrainings.FindAsync(x => x.TypeOfWorkId == id))
            _unitOfWork.TypeOfWorkTrainings.Remove(tt);

        _unitOfWork.TypeOfWorks.Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ComplianceRequirementDto>> GetRequirementsForTypeOfWorkAsync(int typeOfWorkId)
    {
        var tow = await _unitOfWork.TypeOfWorks.GetByIdAsync(typeOfWorkId);
        if (tow is null) return Array.Empty<ComplianceRequirementDto>();

        var items = await _unitOfWork.ComplianceItems.FindAsync(c =>
            c.TypeOfWorkId == typeOfWorkId && c.PersonnelId == null);

        return items
            .OrderBy(c => c.Type)
            .ThenBy(c => c.TitleOrFilePath)
            .Select(c => new ComplianceRequirementDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.TitleOrFilePath
            });
    }

    public async Task<ComplianceRequirementDto?> CreateRequirementForTypeOfWorkAsync(
        int typeOfWorkId,
        CreateComplianceRequirementDto dto)
    {
        var title = (dto.Title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(title))
            return null;

        var tow = await _unitOfWork.TypeOfWorks.GetByIdAsync(typeOfWorkId);
        if (tow is null) return null;

        // Modèle type de travail : uniquement des pièces « Document » (pas de ligne Training auto).
        const string type = "Document";

        var entity = new ComplianceItem
        {
            Type = type,
            TitleOrFilePath = title,
            IsValid = false,
            ValidatedByAI = false,
            PersonnelId = null,
            TypeOfWorkId = typeOfWorkId
        };

        await _unitOfWork.ComplianceItems.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return new ComplianceRequirementDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Title = entity.TitleOrFilePath
        };
    }

    public async Task<bool> DeleteTypeOfWorkRequirementAsync(int requirementId)
    {
        var item = await _unitOfWork.ComplianceItems.GetByIdAsync(requirementId);
        if (item is null) return false;
        if (item.PersonnelId != null || item.TypeOfWorkId is null) return false;

        _unitOfWork.ComplianceItems.Remove(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ZoneDto>> GetZonesAsync()
    {
        var items = await _unitOfWork.Zones.GetAllAsync();
        return items.Select(z => new ZoneDto
        {
            Id = z.Id,
            Name = z.Name,
            Description = z.Description
        });
    }

    public async Task<ZoneDto> CreateZoneAsync(ZoneDto dto)
    {
        var entity = new Zone
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty
        };

        await _unitOfWork.Zones.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = entity.Id;
        return dto;
    }

    public async Task<ZoneDto?> UpdateZoneAsync(int id, ZoneDto dto)
    {
        var entity = await _unitOfWork.Zones.GetByIdAsync(id);
        if (entity is null) return null;

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description ?? string.Empty;

        _unitOfWork.Zones.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return new ZoneDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public async Task<bool?> DeleteZoneAsync(int id)
    {
        var entity = await _unitOfWork.Zones.GetByIdAsync(id);
        if (entity is null) return null;

        if (await _db.InterventionZones.AsNoTracking().AnyAsync(z => z.ZoneId == id))
            return false;

        _unitOfWork.Zones.Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task BlacklistPersonnelAsync(int personnelId, bool isBlacklisted)
    {
        var personnel = await _unitOfWork.Personnels.GetByIdAsync(personnelId);
        if (personnel is null) return;

        personnel.IsBlacklisted = isBlacklisted;
        _unitOfWork.Personnels.Update(personnel);
        await _unitOfWork.SaveChangesAsync();
    }
}

