using VisitFlowAPI.Data;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly VisitFlowDbContext _context;

    public UnitOfWork(VisitFlowDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(context);
        Visits = new GenericRepository<Visit>(context);
        Suppliers = new GenericRepository<Supplier>(context);
        SupplierDocuments = new GenericRepository<SupplierDocument>(context);
        Personnels = new GenericRepository<Personnel>(context);
        Insurances = new GenericRepository<Insurance>(context);
        TypeOfWorks = new GenericRepository<TypeOfWork>(context);
        TypeOfWorkInsurances = new GenericRepository<TypeOfWorkInsurance>(context);
        TypeOfWorkTrainings = new GenericRepository<TypeOfWorkTraining>(context);
        Trainings = new GenericRepository<Training>(context);
        PersonnelTrainings = new GenericRepository<PersonnelTraining>(context);
        Zones = new GenericRepository<Zone>(context);
        Interventions = new GenericRepository<Intervention>(context);
        InterventionPersonnels = new GenericRepository<InterventionPersonnel>(context);
        Documents = new GenericRepository<Document>(context);
        ComplianceItems = new GenericRepository<ComplianceItem>(context);
    }

    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Visit> Visits { get; }
    public IGenericRepository<Supplier> Suppliers { get; }
    public IGenericRepository<SupplierDocument> SupplierDocuments { get; }
    public IGenericRepository<Personnel> Personnels { get; }
    public IGenericRepository<Insurance> Insurances { get; }
    public IGenericRepository<TypeOfWork> TypeOfWorks { get; }
    public IGenericRepository<TypeOfWorkInsurance> TypeOfWorkInsurances { get; }
    public IGenericRepository<TypeOfWorkTraining> TypeOfWorkTrainings { get; }
    public IGenericRepository<Training> Trainings { get; }
    public IGenericRepository<PersonnelTraining> PersonnelTrainings { get; }
    public IGenericRepository<Zone> Zones { get; }
    public IGenericRepository<Intervention> Interventions { get; }
    public IGenericRepository<InterventionPersonnel> InterventionPersonnels { get; }
    public IGenericRepository<Document> Documents { get; }
    public IGenericRepository<ComplianceItem> ComplianceItems { get; }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}
