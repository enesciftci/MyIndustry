using System.Linq.Expressions;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.Repository.Repository;

public interface IGenericRepository<T> where T : Entity
{
    public Task AddAsync(T entity, CancellationToken cancellationToken);
    public  Task AddRange(IEnumerable<T> entity, CancellationToken cancellationToken);

    public void Delete(T entity);
    public  Task Delete(Guid id, CancellationToken cancellationToken);

    public  Task Delete(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    public void DeleteRange(List<T> entities);

    public  Task<List<T>> GetAll(CancellationToken cancellationToken);

    public IQueryable<T> GetAllQuery();

    public  Task<List<T>> GetAll(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    public  Task<T> GetById(Guid id, CancellationToken cancellationToken);

    public  Task<T> GetById(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);

    public void Update(T entity);

    public void UpdateRange(List<T> entity);

    public  Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken);
    Task<bool> Contains(T entity, CancellationToken cancellationToken);
    IQueryable<T> FromSqlRaw(string sql);
    Task<T> GetSingleOrDefault(Guid id, CancellationToken cancellationToken);

}