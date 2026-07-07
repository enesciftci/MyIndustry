using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Repository.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : Entity
{
    private readonly MyIndustryDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(MyIndustryDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddRange(IEnumerable<T> entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddRangeAsync(entity, cancellationToken);
    }

    public void Delete(T entity)
    {
        if (entity == null)
            return;

        var tracked = FindTrackedEntity(entity.Id);
        _dbSet.Remove(tracked ?? entity);
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetById(id, cancellationToken);
        if (entity == null)
            return;

        Delete(entity);
    }

    public async Task Delete(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        var entity = await GetById(predicate, cancellationToken);
        if (entity == null)
            return;

        Delete(entity);
    }

    public void DeleteRange(List<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }


    public async Task<List<T>> GetAll(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken: cancellationToken);
    }

    public IQueryable<T> GetAllQuery()
    {
        return _dbSet.AsNoTracking();
    }

    public async Task<List<T>> GetAll(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<T> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    
    public async Task<T> GetSingleOrDefault(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<T> GetById(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public void Update(T entity)
    {
        var tracked = FindTrackedEntity(entity.Id);
        if (tracked != null && !ReferenceEquals(tracked, entity))
        {
            _context.Entry(tracked).CurrentValues.SetValues(entity);
            _context.Entry(tracked).State = EntityState.Modified;
            return;
        }

        _dbSet.Update(entity);
    }

    public void UpdateRange(List<T> entityList)
    {
        foreach (var entity in entityList)
        {
            entity.ModifiedDate = DateTime.Now;
            Update(entity);
        }
    }

    private T? FindTrackedEntity(Guid id) =>
        _context.ChangeTracker.Entries<T>()
            .FirstOrDefault(e => e.Entity.Id == id)
            ?.Entity;

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(expression, cancellationToken: cancellationToken);
    }

    public Task<bool> Contains(T entity, CancellationToken cancellationToken)
    {
        return _dbSet.ContainsAsync(entity, cancellationToken: cancellationToken);
    }

    [Obsolete("Use parameterized queries via FromSqlInterpolated. Raw SQL with user input is forbidden.")]
    public IQueryable<T> FromSqlRaw(string sql)
    {
        // GÜVENLİK: Sadece sabit/sert kodlanmış SQL veya parametreli FromSqlInterpolated kullanın.
        // Kullanıcı girdisini asla string birleştirmeyin (SQL injection riski).
        return _dbSet.FromSqlRaw(sql);
    }
}