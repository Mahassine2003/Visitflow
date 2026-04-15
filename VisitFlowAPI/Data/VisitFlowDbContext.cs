using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Data;

public class VisitFlowDbContext : DbContext
{
    public VisitFlowDbContext(DbContextOptions<VisitFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierDocument> SupplierDocuments => Set<SupplierDocument>();
    public DbSet<Personnel> Personnels => Set<Personnel>();
    public DbSet<Insurance> Insurances => Set<Insurance>();
    public DbSet<TypeOfWork> TypeOfWorks => Set<TypeOfWork>();
    public DbSet<TypeOfWorkInsurance> TypeOfWorkInsurances => Set<TypeOfWorkInsurance>();
    public DbSet<TypeOfWorkTraining> TypeOfWorkTrainings => Set<TypeOfWorkTraining>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<PersonnelTraining> PersonnelTrainings => Set<PersonnelTraining>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<InterventionZone> InterventionZones => Set<InterventionZone>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<InterventionPlant> InterventionPlants => Set<InterventionPlant>();
    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<InterventionWizardFieldDefinition> InterventionWizardFieldDefinitions => Set<InterventionWizardFieldDefinition>();
    public DbSet<InterventionPersonnel> InterventionPersonnels => Set<InterventionPersonnel>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<ElementType> ElementTypes => Set<ElementType>();
    public DbSet<ElementOption> ElementOptions => Set<ElementOption>();
    public DbSet<InterventionElement> InterventionElements => Set<InterventionElement>();
    public DbSet<Approved> Approveds => Set<Approved>();
    public DbSet<SafetyMeasure> SafetyMeasures => Set<SafetyMeasure>();
    public DbSet<ComplianceItem> ComplianceItems => Set<ComplianceItem>();
    public DbSet<BlacklistRequest> BlacklistRequests => Set<BlacklistRequest>();
    public DbSet<Validation> Validations => Set<Validation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Session>()
            .Property(s => s.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Visit>()
            .HasIndex(v => v.VisitId)
            .IsUnique();

        modelBuilder.Entity<Visit>()
            .HasOne(v => v.User)
            .WithMany(u => u.Visits)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Personnel>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Personnel)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SupplierDocument>()
            .HasOne(d => d.Supplier)
            .WithMany(s => s.SupplierDocuments)
            .HasForeignKey(d => d.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Personnel>()
            .HasOne(p => p.TypeOfWork)
            .WithMany(t => t.Personnels)
            .HasForeignKey(p => p.TypeOfWorkId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Personnel>()
            .HasIndex(p => p.Cin)
            .IsUnique();

        modelBuilder.Entity<Insurance>()
            .HasOne(i => i.Personnel)
            .WithMany(p => p.Insurances)
            .HasForeignKey(i => i.PersonnelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TypeOfWorkInsurance>()
            .HasKey(ti => new { ti.TypeOfWorkId, ti.InsuranceId });

        modelBuilder.Entity<TypeOfWorkInsurance>()
            .HasOne(ti => ti.TypeOfWork)
            .WithMany(t => t.TypeOfWorkInsurances)
            .HasForeignKey(ti => ti.TypeOfWorkId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TypeOfWorkInsurance>()
            .HasOne(ti => ti.Insurance)
            .WithMany(i => i.TypeOfWorkInsurances)
            .HasForeignKey(ti => ti.InsuranceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TypeOfWorkTraining>()
            .HasKey(tt => new { tt.TypeOfWorkId, tt.TrainingId });

        modelBuilder.Entity<TypeOfWorkTraining>()
            .HasOne(tt => tt.TypeOfWork)
            .WithMany(t => t.TypeOfWorkTrainings)
            .HasForeignKey(tt => tt.TypeOfWorkId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TypeOfWorkTraining>()
            .HasOne(tt => tt.Training)
            .WithMany(t => t.TypeOfWorkTrainings)
            .HasForeignKey(tt => tt.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PersonnelTraining>()
            .HasKey(pt => new { pt.PersonnelId, pt.TrainingId });

        modelBuilder.Entity<PersonnelTraining>()
            .HasOne(pt => pt.Personnel)
            .WithMany(p => p.PersonnelTrainings)
            .HasForeignKey(pt => pt.PersonnelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PersonnelTraining>()
            .HasOne(pt => pt.Training)
            .WithMany(t => t.PersonnelTrainings)
            .HasForeignKey(pt => pt.TrainingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterventionPersonnel>()
            .HasKey(ip => new { ip.InterventionId, ip.PersonnelId });

        modelBuilder.Entity<InterventionPersonnel>()
            .HasOne(ip => ip.Intervention)
            .WithMany(i => i.InterventionPersonnels)
            .HasForeignKey(ip => ip.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterventionPersonnel>()
            .HasOne(ip => ip.Personnel)
            .WithMany(p => p.InterventionPersonnels)
            .HasForeignKey(ip => ip.PersonnelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.Supplier)
            .WithMany(s => s.Interventions)
            .HasForeignKey(i => i.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.TypeOfWork)
            .WithMany(t => t.Interventions)
            .HasForeignKey(i => i.TypeOfWorkId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.CreatedByUser)
            .WithMany(u => u.CreatedInterventions)
            .HasForeignKey(i => i.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.Visit)
            .WithMany(v => v.Interventions)
            .HasForeignKey(i => i.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Intervention>()
            .Property(i => i.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Intervention>()
            .HasOne(i => i.Document)
            .WithOne(d => d.Intervention)
            .HasForeignKey<Document>(d => d.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.InterventionId)
            .IsUnique();

        modelBuilder.Entity<InterventionZone>()
            .HasKey(iz => new { iz.InterventionId, iz.ZoneId });

        modelBuilder.Entity<InterventionZone>()
            .HasOne(iz => iz.Intervention)
            .WithMany(i => i.InterventionZones)
            .HasForeignKey(iz => iz.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterventionZone>()
            .HasOne(iz => iz.Zone)
            .WithMany(z => z.InterventionZones)
            .HasForeignKey(iz => iz.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InterventionPlant>()
            .HasKey(ip => new { ip.InterventionId, ip.PlantId });

        modelBuilder.Entity<InterventionPlant>()
            .HasOne(ip => ip.Intervention)
            .WithMany(i => i.InterventionPlants)
            .HasForeignKey(ip => ip.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterventionPlant>()
            .HasOne(ip => ip.Plant)
            .WithMany(p => p.InterventionPlants)
            .HasForeignKey(ip => ip.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Form>()
            .HasOne(f => f.Document)
            .WithMany(d => d.Forms)
            .HasForeignKey(f => f.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ElementOption>()
            .HasOne(e => e.ElementType)
            .WithMany(t => t.ElementOptions)
            .HasForeignKey(e => e.ElementTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterventionElement>()
            .Property(e => e.Context)
            .HasConversion<string>();

        modelBuilder.Entity<InterventionElement>()
            .HasOne(e => e.Intervention)
            .WithMany(i => i.InterventionElements)
            .HasForeignKey(e => e.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterventionElement>()
            .HasOne(e => e.Form)
            .WithMany(f => f.InterventionElements)
            .HasForeignKey(e => e.FormId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InterventionElement>()
            .HasOne(e => e.ElementType)
            .WithMany(t => t.InterventionElements)
            .HasForeignKey(e => e.ElementTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Approved>()
            .Property(a => a.ApproverRole)
            .HasConversion<string>();

        modelBuilder.Entity<Approved>()
            .HasOne(a => a.Intervention)
            .WithMany(i => i.Approved)
            .HasForeignKey(a => a.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SafetyMeasure>()
            .HasOne(s => s.Intervention)
            .WithMany(i => i.SafetyMeasures)
            .HasForeignKey(s => s.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComplianceItem>()
            .HasOne(c => c.Personnel)
            .WithMany(p => p.ComplianceItems)
            .HasForeignKey(c => c.PersonnelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComplianceItem>()
            .HasOne(c => c.TypeOfWork)
            .WithMany(t => t.ComplianceItems)
            .HasForeignKey(c => c.TypeOfWorkId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComplianceItem>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_ComplianceItem_Owner",
                "([PersonnelId] IS NOT NULL AND [TypeOfWorkId] IS NULL) OR ([PersonnelId] IS NULL AND [TypeOfWorkId] IS NOT NULL)"));

        modelBuilder.Entity<BlacklistRequest>()
            .Property(b => b.Status)
            .HasConversion<string>();

        modelBuilder.Entity<BlacklistRequest>()
            .HasOne(b => b.Personnel)
            .WithMany(p => p.BlacklistRequests)
            .HasForeignKey(b => b.PersonnelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BlacklistRequest>()
            .HasOne(b => b.ReviewedByUser)
            .WithMany(u => u.ReviewedBlacklistRequests)
            .HasForeignKey(b => b.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Validation>()
            .HasOne(v => v.Intervention)
            .WithMany()
            .HasForeignKey(v => v.InterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterventionWizardFieldDefinition>()
            .HasIndex(x => x.Key)
            .IsUnique();

        modelBuilder.Entity<InterventionWizardFieldDefinition>()
            .Property(x => x.FieldType)
            .HasConversion<int>();

        modelBuilder.Entity<InterventionWizardFieldDefinition>()
            .Property(x => x.CreationMode)
            .HasConversion<int>();
    }
}
