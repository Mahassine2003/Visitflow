namespace VisitFlowAPI.Models;

public class TypeOfWorkTraining
{
    public int TypeOfWorkId { get; set; }
    public int TrainingId { get; set; }

    // Navigation
    public TypeOfWork TypeOfWork { get; set; } = null!;
    public Training Training { get; set; } = null!;
}
