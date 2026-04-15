using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;

namespace VisitFlowAPI.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly VisitFlowDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(VisitFlowDbContext context)
    {
        Context = context;
        DbSet = Context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await DbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.Where(predicate).ToListAsync();

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);
}

