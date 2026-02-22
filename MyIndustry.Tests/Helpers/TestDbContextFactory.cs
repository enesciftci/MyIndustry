using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Tests.Helpers;

public static class TestDbContextFactory
{
    public static MyIndustryDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<MyIndustryDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new MyIndustryDbContext(options);
    }
}
