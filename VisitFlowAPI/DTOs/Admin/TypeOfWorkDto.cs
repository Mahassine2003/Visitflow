namespace VisitFlowAPI.DTOs.Admin;

public class TypeOfWorkDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresInsurance { get; set; }
    /// <summary>
    /// Si null : non précisé (utile pour les updates partiels).
    /// Côté liste : toujours rempli.
    /// </summary>
    public bool? RequiresTraining { get; set; }

    /// <summary>
    /// Libellé de la formation (utilisé lors de la création / activation).
    /// </summary>
    public string? TrainingTitle { get; set; }
}

