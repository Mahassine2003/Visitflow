using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Infrastructure.Seed;

public static class SeedDataExtensions
{
    public static async Task SeedVisitFlowAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VisitFlowDbContext>();

        // Seed should never crash the app if the SQL connection is misconfigured.
        // If connection/auth fails, the API can still start (endpoints will fail until fixed).
        try
        {
            await db.Database.CanConnectAsync();
        }
        catch
        {
            return;
        }

        if (!db.Zones.Any())
        {
            db.Zones.AddRange(
                new Zone { Name = "Zone A", Description = "Default" },
                new Zone { Name = "Zone B", Description = "Default" }
            );
        }

        if (!db.TypeOfWorks.Any())
        {
            db.TypeOfWorks.AddRange(
                new TypeOfWork { Name = "Work at Height", Description = "High-risk work", RequiresInsurance = true },
                new TypeOfWork { Name = "Maintenance", Description = "Maintenance operations", RequiresInsurance = true }
            );
        }

        foreach (var name in new[] { "TFZ", "TAC1", "TAC2" })
        {
            if (!await db.Plants.AnyAsync(p => p.Name == name))
                db.Plants.Add(new Plant { Name = name, Description = string.Empty });
        }

        await db.SaveChangesAsync();

        // Exigences (Assurance RC, décennale, formations, etc.) : uniquement via l’admin
        // Types of Work → Requirements, pas de seed automatique.

        if (!db.Trainings.Any())
        {
            db.Trainings.Add(new Training { Title = "Height Safety", Description = "Mandatory for work at height" });
        }

        if (!db.ElementTypes.Any())
        {
            db.ElementTypes.AddRange(
                new ElementType { Name = "Élément générique" },
                new ElementType { Name = "Équipement" },
                new ElementType { Name = "Risque identifié" }
            );
        }

        SeedInterventionWizardFieldDefinitionsIfMissing(db);

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Default keys match the frontend mapping to <see cref="InterventionCreateDto"/> (title, zones, times, etc.).
    /// </summary>
    private static void SeedInterventionWizardFieldDefinitionsIfMissing(VisitFlowDbContext db)
    {
        void Ensure(string key, string label, InterventionWizardFieldType ft, int sortOrder, bool isRequired)
        {
            if (db.InterventionWizardFieldDefinitions.Any(x => x.Key == key)) return;
            db.InterventionWizardFieldDefinitions.Add(new InterventionWizardFieldDefinition
            {
                Key = key,
                Label = label,
                FieldType = ft,
                SortOrder = sortOrder,
                IsRequired = isRequired,
                OptionsJson = null,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        Ensure("title", "Title", InterventionWizardFieldType.Text, 10, true);
        Ensure("description", "Description", InterventionWizardFieldType.Text, 20, true);
        Ensure("ppi", "PPI", InterventionWizardFieldType.Text, 30, false);
        Ensure("zoneIds", "Zones", InterventionWizardFieldType.Text, 40, true);
        Ensure("typeOfWorkId", "Work type", InterventionWizardFieldType.Text, 50, true);
        Ensure("startTime", "Start time", InterventionWizardFieldType.Time, 60, true);
        Ensure("endTime", "End time", InterventionWizardFieldType.Time, 70, true);
    }

}
