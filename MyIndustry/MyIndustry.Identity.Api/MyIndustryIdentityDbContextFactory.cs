using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyIndustry.Identity.Repository;

namespace MyIndustry.Identity.Api;

public class MyIndustryIdentityDbContextFactory : IDesignTimeDbContextFactory<MyIndustryIdentityDbContext>
{
    public MyIndustryIdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // EF komutu hangi dizinde çalışıyorsa orayı baz alır
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MyIndustryIdentityDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("MyIndustryIdentityDb")); // ya da hangi veritabanını kullanıyorsan

        return new MyIndustryIdentityDbContext(optionsBuilder.Options);
    }
}