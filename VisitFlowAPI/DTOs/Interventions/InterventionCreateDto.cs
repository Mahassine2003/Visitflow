namespace VisitFlowAPI.DTOs.Interventions;

public class InterventionCreateDto
{
    /// <summary>Usine (site) : une intervention est rattachée à une plante.</summary>
    public int PlantId { get; set; }

    public string Title { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public List<int> ZoneIds { get; set; } = new();
    public int TypeOfWorkId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Ppi { get; set; } = string.Empty;
    public int MinPersonnel { get; set; }
    public int MinZone { get; set; }

    public string? FirePermitDetails { get; set; }
    public string? HeightPermitDetails { get; set; }

    /// <summary>Valeurs des champs dynamiques (clé = définition.Key).</summary>
    public Dictionary<string, string>? CustomFieldValues { get; set; }
}
