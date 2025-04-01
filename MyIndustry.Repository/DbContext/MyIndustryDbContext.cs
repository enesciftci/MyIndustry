using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.Repository.DbContext;

public class MyIndustryDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    
    public MyIndustryDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<Commission> Commissions { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Purchaser> Purchasers { get; set; }
    public DbSet<PurchaserInfo> PurchaserInfos { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<SellerInfo> SellerInfos { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
}