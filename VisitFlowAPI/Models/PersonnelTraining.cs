namespace VisitFlowAPI.Models;

public class PersonnelTraining
{
    public int PersonnelId { get; set; }
    public int TrainingId { get; set; }
    public bool Completed { get; set; }
    public DateOnly? CompletionDate { get; set; }

    // Navigation
    public Personnel Personnel { get; set; } = null!;
    public Training Training { get; set; } = null!;
}
