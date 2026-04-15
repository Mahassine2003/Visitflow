namespace VisitFlowAPI.DTOs.Interventions;

public class InterventionDto
{
    public int Id { get; set; }

    /// <summary>Identifiant de l’usine associée (première entrée InterventionPlants).</summary>
    public int? PlantId { get; set; }

    public string? PlantName { get; set; }

    public string VisitKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public List<int> ZoneIds { get; set; } = new();
    public int TypeOfWorkId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HSEApprovalStatus { get; set; } = string.Empty;
    public string Ppi { get; set; } = string.Empty;
    public int MinPersonnel { get; set; }
    public int MinZone { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public string? FirePermitDetails { get; set; }
    public string? HeightPermitDetails { get; set; }
    public string? HseFormDetails { get; set; }
    public bool IsHseValidated { get; set; }

    /// <summary>Valeurs des champs dynamiques (étape 1), si présentes en base.</summary>
    public Dictionary<string, string>? CustomFieldValues { get; set; }
}
