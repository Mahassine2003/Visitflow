namespace VisitFlowAPI.Models;

public class Training
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<TypeOfWorkTraining> TypeOfWorkTrainings { get; set; } = new List<TypeOfWorkTraining>();
    public ICollection<PersonnelTraining> PersonnelTrainings { get; set; } = new List<PersonnelTraining>();
}
