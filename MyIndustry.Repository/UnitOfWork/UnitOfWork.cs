using MyIndustry.Repository.DbContext;

namespace MyIndustry.Repository.UnitOfWork;

public class UnitOfWork(MyIndustryDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        context.SetAuditFields();
        await context.SaveChangesAsync(cancellationToken);
    }
}