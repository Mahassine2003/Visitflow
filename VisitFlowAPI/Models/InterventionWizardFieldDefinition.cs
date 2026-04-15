namespace VisitFlowAPI.Models;

/// <summary>Origine du champ : définition libre ou colonne liée à une table existante.</summary>
public enum InterventionWizardFieldCreationMode
{
    /// <summary>Nouveau champ : ligne dans cette table + valeurs dans CustomFieldsJson de l’intervention.</summary>
    CustomField = 0,

    /// <summary>Liste (ou saisie) basée sur une table/colonne existante (valeurs distinctes chargées depuis la base).</summary>
    DatabaseBinding = 1
}

/// <summary>Types de champs dynamiques pour l’étape 1 du wizard intervention.</summary>
public enum InterventionWizardFieldType
{
    /// <summary>Champ texte libre.</summary>
    Text = 0,
    /// <summary>Valeur numérique.</summary>
    Number = 1,
    Date = 2,
    Time = 3,
    /// <summary>Liste déroulante ; les choix sont dans <see cref="InterventionWizardFieldDefinition.OptionsJson"/>.</summary>
    Select = 4
}

/// <summary>Définition d’un champ supplémentaire (configurable par l’utilisateur).</summary>
public class InterventionWizardFieldDefinition
{
    public int Id { get; set; }

    /// <summary>Clé stable (slug), unique.</summary>
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public InterventionWizardFieldType FieldType { get; set; } = InterventionWizardFieldType.Text;

    public InterventionWizardFieldCreationMode CreationMode { get; set; } = InterventionWizardFieldCreationMode.CustomField;

    /// <summary>Si <see cref="CreationMode"/> est <see cref="InterventionWizardFieldCreationMode.DatabaseBinding"/> : schéma SQL (ex. dbo).</summary>
    public string? SourceSchema { get; set; }

    /// <summary>Table source (nom simple, ex. Personnels).</summary>
    public string? SourceTable { get; set; }

    /// <summary>Colonne source (ex. FullName).</summary>
    public string? SourceColumn { get; set; }

    /// <summary>JSON array of option labels, e.g. <c>["A","B"]</c>. Used when <see cref="FieldType"/> is <see cref="InterventionWizardFieldType.Select"/>.</summary>
    public string? OptionsJson { get; set; }

    public int SortOrder { get; set; }

    public bool IsRequired { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
