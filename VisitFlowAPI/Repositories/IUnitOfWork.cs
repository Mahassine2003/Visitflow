using VisitFlowAPI.Models;

namespace VisitFlowAPI.Repositories;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Visit> Visits { get; }
    IGenericRepository<Supplier> Suppliers { get; }
    IGenericRepository<SupplierDocument> SupplierDocuments { get; }
    IGenericRepository<Personnel> Personnels { get; }
    IGenericRepository<Insurance> Insurances { get; }
    IGenericRepository<TypeOfWork> TypeOfWorks { get; }
    IGenericRepository<TypeOfWorkInsurance> TypeOfWorkInsurances { get; }
    IGenericRepository<TypeOfWorkTraining> TypeOfWorkTrainings { get; }
    IGenericRepository<Training> Trainings { get; }
    IGenericRepository<PersonnelTraining> PersonnelTrainings { get; }
    IGenericRepository<Zone> Zones { get; }
    IGenericRepository<Intervention> Interventions { get; }
    IGenericRepository<InterventionPersonnel> InterventionPersonnels { get; }
    IGenericRepository<Document> Documents { get; }
    IGenericRepository<ComplianceItem> ComplianceItems { get; }

    Task<int> SaveChangesAsync();
}
