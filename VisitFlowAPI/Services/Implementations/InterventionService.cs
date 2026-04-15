using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Interventions;
using VisitFlowAPI.Models;
using VisitFlowAPI.Repositories;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Services.Implementations;

public class InterventionService : IInterventionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VisitFlowDbContext _db;

    public InterventionService(IUnitOfWork unitOfWork, VisitFlowDbContext db)
    {
        _unitOfWork = unitOfWork;
        _db = db;
    }

    public async Task<InterventionDto> CreateInterventionAsync(InterventionCreateDto dto, string createdBy, int userId)
    {
        if (!await _db.Plants.AsNoTracking().AnyAsync(p => p.Id == dto.PlantId))
            throw new InvalidOperationException("Plant not found.");

        var visit = new Visit
        {
            VisitId = Guid.NewGuid().ToString("N"),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Visits.AddAsync(visit);
        await _db.SaveChangesAsync();

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var createdByName = user?.FullName ?? createdBy;

        var entity = new Intervention
        {
            Title = dto.Title,
            VisitId = visit.Id,
            SupplierId = dto.SupplierId,
            TypeOfWorkId = dto.TypeOfWorkId,
            Description = dto.Description,
            StartDate = dto.StartDate.ToDateTime(dto.StartTime),
            EndDate = dto.EndDate.ToDateTime(dto.EndTime),
            Status = InterventionStatus.Pending,
            CreatedByUserId = userId,
            CreatedBy = createdByName,
            Ppi = dto.Ppi,
            MinPersonnel = dto.MinPersonnel,
            MinZone = dto.MinZone,
            FirePermitDetails = dto.FirePermitDetails,
            HeightPermitDetails = dto.HeightPermitDetails,
            CustomFieldsJson = SerializeFieldValues(dto.CustomFieldValues)
        };

        await _unitOfWork.Interventions.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        foreach (var zid in dto.ZoneIds.Distinct())
        {
            await _db.InterventionZones.AddAsync(new InterventionZone
            {
                InterventionId = entity.Id,
                ZoneId = zid
            });
        }

        await _db.InterventionPlants.AddAsync(new InterventionPlant
        {
            InterventionId = entity.Id,
            PlantId = dto.PlantId
        });

        await _db.SaveChangesAsync();

        return (await ToDtoAsync(entity.Id))!;
    }

    public async Task<IEnumerable<InterventionDto>> GetInterventionsAsync()
    {
        var entities = await _db.Interventions
            .AsNoTracking()
            .Include(i => i.Visit)
            .OrderByDescending(i => i.Id)
            .ToListAsync();

        var interventionIds = entities.Select(e => e.Id).ToList();
        var zoneRows = await _db.InterventionZones
            .AsNoTracking()
            .Where(iz => interventionIds.Contains(iz.InterventionId))
            .ToListAsync();

        var zoneLookup = zoneRows
            .GroupBy(z => z.InterventionId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ZoneId).ToList());

        var plantRows = await (
            from ip in _db.InterventionPlants.AsNoTracking()
            join p in _db.Plants.AsNoTracking() on ip.PlantId equals p.Id
            where interventionIds.Contains(ip.InterventionId)
            select new { ip.InterventionId, ip.PlantId, PlantName = p.Name }
        ).ToListAsync();
        var plantLookup = plantRows
            .GroupBy(x => x.InterventionId)
            .ToDictionary(g => g.Key, g => g.First());

        return entities.Select(entity =>
        {
            zoneLookup.TryGetValue(entity.Id, out var zids);
            plantLookup.TryGetValue(entity.Id, out var plantInfo);
            return new InterventionDto
            {
                Id = entity.Id,
                PlantId = plantInfo?.PlantId,
                PlantName = plantInfo?.PlantName,
                VisitKey = entity.Visit.VisitId,
                Title = entity.Title,
                SupplierId = entity.SupplierId,
                ZoneIds = zids ?? new List<int>(),
                TypeOfWorkId = entity.TypeOfWorkId,
                Description = entity.Description,
                StartDate = DateOnly.FromDateTime(entity.StartDate),
                EndDate = DateOnly.FromDateTime(entity.EndDate),
                StartTime = TimeOnly.FromDateTime(entity.StartDate),
                EndTime = TimeOnly.FromDateTime(entity.EndDate),
                Status = entity.Status.ToString(),
                HSEApprovalStatus = entity.IsHSEValidated ? "Validated" : "Pending",
                Ppi = entity.Ppi,
                MinPersonnel = entity.MinPersonnel,
                MinZone = entity.MinZone,
                CreatedBy = entity.CreatedBy,
                FirePermitDetails = entity.FirePermitDetails,
                HeightPermitDetails = entity.HeightPermitDetails,
                HseFormDetails = entity.HseFormDetails,
                IsHseValidated = entity.IsHSEValidated,
                CustomFieldValues = DeserializeFieldValues(entity.CustomFieldsJson)
            };
        });
    }

    public async Task<InterventionDetailDto?> GetInterventionDetailAsync(int id)
    {
        var entity = await _db.Interventions
            .AsNoTracking()
            .Include(i => i.Visit)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (entity is null) return null;

        var zoneIds = await _db.InterventionZones
            .Where(iz => iz.InterventionId == id)
            .Select(iz => iz.ZoneId)
            .ToListAsync();

        var plantInfo = await (
            from ip in _db.InterventionPlants.AsNoTracking()
            join p in _db.Plants.AsNoTracking() on ip.PlantId equals p.Id
            where ip.InterventionId == id
            select new { ip.PlantId, PlantName = p.Name }
        ).FirstOrDefaultAsync();

        var elementRows = await _db.InterventionElements
            .AsNoTracking()
            .Include(e => e.ElementType)
                .ThenInclude(et => et!.ElementOptions)
            .Where(e => e.InterventionId == id)
            .OrderBy(e => e.Id)
            .ToListAsync();

        var elements = elementRows.Select(e => new InterventionElementDto
        {
            Id = e.Id,
            ElementTypeId = e.ElementTypeId,
            ElementTypeName = e.ElementType.Name,
            Label = e.Label,
            IsChecked = e.IsChecked,
            Context = e.Context.ToString(),
            Options = e.ElementType.ElementOptions
                .OrderBy(o => o.Id)
                .Select(o => new ElementOptionDto { Id = o.Id, Label = o.Label })
                .ToList(),
            FieldValues = DeserializeFieldValues(e.FieldValuesJson)
        }).ToList();

        var measures = await _db.SafetyMeasures
            .AsNoTracking()
            .Where(s => s.InterventionId == id)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SafetyMeasureDto
            {
                Id = s.Id,
                Description = s.Description,
                AddedBy = s.AddedBy,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return new InterventionDetailDto
        {
            Id = entity.Id,
            PlantId = plantInfo?.PlantId,
            PlantName = plantInfo?.PlantName,
            VisitKey = entity.Visit.VisitId,
            Title = entity.Title,
            SupplierId = entity.SupplierId,
            ZoneIds = zoneIds,
            TypeOfWorkId = entity.TypeOfWorkId,
            Description = entity.Description,
            StartDate = DateOnly.FromDateTime(entity.StartDate),
            EndDate = DateOnly.FromDateTime(entity.EndDate),
            StartTime = TimeOnly.FromDateTime(entity.StartDate),
            EndTime = TimeOnly.FromDateTime(entity.EndDate),
            Status = entity.Status.ToString(),
            HSEApprovalStatus = entity.IsHSEValidated ? "Validated" : "Pending",
            Ppi = entity.Ppi,
            MinPersonnel = entity.MinPersonnel,
            MinZone = entity.MinZone,
            CreatedBy = entity.CreatedBy,
            FirePermitDetails = entity.FirePermitDetails,
            HeightPermitDetails = entity.HeightPermitDetails,
            HseFormDetails = entity.HseFormDetails,
            IsHseValidated = entity.IsHSEValidated,
            CustomFieldValues = DeserializeFieldValues(entity.CustomFieldsJson),
            Elements = elements,
            SafetyMeasures = measures
        };
    }

    public async Task<InterventionElementDto> AddInterventionElementAsync(int interventionId, AddInterventionElementDto dto)
    {
        var exists = await _db.Interventions.AnyAsync(i => i.Id == interventionId);
        if (!exists) throw new InvalidOperationException("Intervention not found");

        var ctx = Enum.IsDefined(typeof(ElementContext), dto.Context)
            ? (ElementContext)dto.Context
            : ElementContext.Intervention;

        var row = new InterventionElement
        {
            InterventionId = interventionId,
            ElementTypeId = dto.ElementTypeId,
            Label = dto.Label,
            IsChecked = dto.IsChecked,
            Context = ctx
        };
        _db.InterventionElements.Add(row);
        await _db.SaveChangesAsync();

        var et = await _db.ElementTypes
            .AsNoTracking()
            .Include(t => t.ElementOptions)
            .FirstAsync(t => t.Id == dto.ElementTypeId);
        return new InterventionElementDto
        {
            Id = row.Id,
            ElementTypeId = row.ElementTypeId,
            ElementTypeName = et.Name,
            Label = row.Label,
            IsChecked = row.IsChecked,
            Context = row.Context.ToString(),
            Options = et.ElementOptions.OrderBy(o => o.Id)
                .Select(o => new ElementOptionDto { Id = o.Id, Label = o.Label })
                .ToList(),
            FieldValues = null
        };
    }

    public async Task<InterventionElementDto> UpdateInterventionElementFieldsAsync(
        int interventionId,
        int elementId,
        UpdateInterventionElementFieldsDto dto)
    {
        var row = await _db.InterventionElements
            .Include(e => e.ElementType)
                .ThenInclude(et => et!.ElementOptions)
            .FirstOrDefaultAsync(e => e.Id == elementId && e.InterventionId == interventionId);
        if (row is null) throw new InvalidOperationException("Intervention element not found");

        row.FieldValuesJson = SerializeFieldValues(dto.FieldValues);
        await _db.SaveChangesAsync();

        return new InterventionElementDto
        {
            Id = row.Id,
            ElementTypeId = row.ElementTypeId,
            ElementTypeName = row.ElementType.Name,
            Label = row.Label,
            IsChecked = row.IsChecked,
            Context = row.Context.ToString(),
            Options = row.ElementType.ElementOptions.OrderBy(o => o.Id)
                .Select(o => new ElementOptionDto { Id = o.Id, Label = o.Label })
                .ToList(),
            FieldValues = DeserializeFieldValues(row.FieldValuesJson)
        };
    }

    public async Task<SafetyMeasureDto> AddSafetyMeasureAsync(int interventionId, string description, string addedBy)
    {
        var exists = await _db.Interventions.AnyAsync(i => i.Id == interventionId);
        if (!exists) throw new InvalidOperationException("Intervention not found");

        var row = new SafetyMeasure
        {
            InterventionId = interventionId,
            Description = description,
            AddedBy = addedBy,
            CreatedAt = DateTime.UtcNow
        };
        _db.SafetyMeasures.Add(row);
        await _db.SaveChangesAsync();

        return new SafetyMeasureDto
        {
            Id = row.Id,
            Description = row.Description,
            AddedBy = row.AddedBy,
            CreatedAt = row.CreatedAt
        };
    }

    public async Task UpdateWorkflowAsync(int interventionId, UpdateInterventionWorkflowDto dto)
    {
        var entity = await _db.Interventions.FirstOrDefaultAsync(i => i.Id == interventionId);
        if (entity is null) throw new InvalidOperationException("Intervention not found");

        if (dto.FirePermitDetails != null) entity.FirePermitDetails = dto.FirePermitDetails;
        if (dto.HeightPermitDetails != null) entity.HeightPermitDetails = dto.HeightPermitDetails;
        if (dto.HseFormDetails != null) entity.HseFormDetails = dto.HseFormDetails;
        if (dto.HseComment != null) entity.HSEComment = dto.HseComment;

        await _db.SaveChangesAsync();
    }

    public async Task AssignPersonnelAsync(int interventionId, IEnumerable<int> personnelIds)
    {
        var intervention = await _unitOfWork.Interventions.GetByIdAsync(interventionId);
        if (intervention is null) return;

        foreach (var pid in personnelIds)
        {
            var link = new InterventionPersonnel
            {
                InterventionId = interventionId,
                PersonnelId = pid
            };
            await _unitOfWork.InterventionPersonnels.AddAsync(link);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ApproveHseAsync(int interventionId, bool approved, string? approverName = null)
    {
        var intervention = await _db.Interventions.FirstOrDefaultAsync(i => i.Id == interventionId);
        if (intervention is null) return;

        intervention.IsHSEValidated = approved;
        intervention.Status = approved ? InterventionStatus.Validated : InterventionStatus.Rejected;

        if (approved)
        {
            await _db.Approveds.AddAsync(new Approved
            {
                InterventionId = interventionId,
                ApproverName = approverName ?? "HSE",
                ApproverRole = UserRole.HSE,
                RequiredApproversCount = 1,
                ApprovalDate = DateTime.UtcNow
            });
        }

        _unitOfWork.Interventions.Update(intervention);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<InterventionDto?> ToDtoAsync(int id)
    {
        var entity = await _db.Interventions
            .AsNoTracking()
            .Include(i => i.Visit)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (entity is null) return null;

        var zoneIds = await _db.InterventionZones
            .Where(iz => iz.InterventionId == id)
            .Select(iz => iz.ZoneId)
            .ToListAsync();

        var plantInfo = await (
            from ip in _db.InterventionPlants.AsNoTracking()
            join p in _db.Plants.AsNoTracking() on ip.PlantId equals p.Id
            where ip.InterventionId == id
            select new { ip.PlantId, PlantName = p.Name }
        ).FirstOrDefaultAsync();

        return new InterventionDto
        {
            Id = entity.Id,
            PlantId = plantInfo?.PlantId,
            PlantName = plantInfo?.PlantName,
            VisitKey = entity.Visit.VisitId,
            Title = entity.Title,
            SupplierId = entity.SupplierId,
            ZoneIds = zoneIds,
            TypeOfWorkId = entity.TypeOfWorkId,
            Description = entity.Description,
            StartDate = DateOnly.FromDateTime(entity.StartDate),
            EndDate = DateOnly.FromDateTime(entity.EndDate),
            StartTime = TimeOnly.FromDateTime(entity.StartDate),
            EndTime = TimeOnly.FromDateTime(entity.EndDate),
            Status = entity.Status.ToString(),
            HSEApprovalStatus = entity.IsHSEValidated ? "Validated" : "Pending",
            Ppi = entity.Ppi,
            MinPersonnel = entity.MinPersonnel,
            MinZone = entity.MinZone,
            CreatedBy = entity.CreatedBy,
            FirePermitDetails = entity.FirePermitDetails,
            HeightPermitDetails = entity.HeightPermitDetails,
            HseFormDetails = entity.HseFormDetails,
            IsHseValidated = entity.IsHSEValidated,
            CustomFieldValues = DeserializeFieldValues(entity.CustomFieldsJson)
        };
    }

    private static Dictionary<string, string>? DeserializeFieldValues(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeFieldValues(Dictionary<string, string>? values)
    {
        if (values is null || values.Count == 0) return null;
        return JsonSerializer.Serialize(values);
    }
}
