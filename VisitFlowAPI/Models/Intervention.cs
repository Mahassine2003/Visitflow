namespace VisitFlowAPI.Models;

public enum InterventionStatus
{
    Pending,
    Validated,
    Rejected
}

public class Intervention
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public InterventionStatus Status { get; set; } = InterventionStatus.Pending;
    public bool IsHSEValidated { get; set; }
    public string HSEComment { get; set; } = string.Empty;

    /// <summary>Fire permit step (text / checklist notes).</summary>
    public string? FirePermitDetails { get; set; }

    /// <summary>Height permit step.</summary>
    public string? HeightPermitDetails { get; set; }

    /// <summary>HSE form content (after HSE validation workflow).</summary>
    public string? HseFormDetails { get; set; }

    public string Ppi { get; set; } = string.Empty;

    /// <summary>Valeurs des champs dynamiques (étape 1), JSON { "key": "value", ... }.</summary>
    public string? CustomFieldsJson { get; set; }

    public int MinPersonnel { get; set; }
    public int MinZone { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public int VisitId { get; set; }
    public int SupplierId { get; set; }
    public int TypeOfWorkId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Visit Visit { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public TypeOfWork TypeOfWork { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;

    public Document? Document { get; set; }
    public ICollection<InterventionZone> InterventionZones { get; set; } = new List<InterventionZone>();
    public ICollection<InterventionPersonnel> InterventionPersonnels { get; set; } = new List<InterventionPersonnel>();
    public ICollection<Approved> Approved { get; set; } = new List<Approved>();
    public ICollection<InterventionElement> InterventionElements { get; set; } = new List<InterventionElement>();
    public ICollection<SafetyMeasure> SafetyMeasures { get; set; } = new List<SafetyMeasure>();
    public ICollection<InterventionPlant> InterventionPlants { get; set; } = new List<InterventionPlant>();
}
