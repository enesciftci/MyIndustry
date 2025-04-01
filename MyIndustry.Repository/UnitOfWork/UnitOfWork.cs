using MyIndustry.Repository.DbContext;

namespace MyIndustry.Repository.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private MyIndustryDbContext _context;
    public UnitOfWork(MyIndustryDbContext context)
    {
        _context = context;
    }
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}