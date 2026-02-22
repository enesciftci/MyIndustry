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

        AddSubscriptionRenewalHistoryConfig(modelBuilder);
        AddServiceConfig(modelBuilder);
        AddCampaignUsageConfig(modelBuilder);
        AddSubscriptionCampaignConfig(modelBuilder);
        AddSubCategoryConfig(modelBuilder);
        AddServiceViewLogConfig(modelBuilder);
        AddSellerInfoConfig(modelBuilder);
        AddSellerSubscriptionConfig(modelBuilder);
        AddSellerAddressConfig(modelBuilder);
        AddMessageConfig(modelBuilder);
        AddLocationConfig(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
    // public DbSet<Commission> Commissions { get; set; }
    // public DbSet<Contract> Contracts { get; set; }
    public DbSet<SellerSubscription> SellerSubscriptions { get; set; }
    public DbSet<SubscriptionCampaign> SubscriptionCampaigns { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<ServiceViewLog> ServiceViewLogs { get; set; }
    public DbSet<SubscriptionRenewalHistory> SubscriptionRenewalHistories { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<SellerInfo> SellerInfos { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Category> Categories { get; set; }
    // public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<Address> Adresses { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Message> Messages { get; set; }
    
    // Location tables
    public DbSet<City> Cities { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Neighborhood> Neighborhoods { get; set; }

    private void AddSellerAddressConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>()
            .HasOne(p => p.Seller)
            .WithMany(p => p.Addresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    private void AddSubscriptionRenewalHistoryConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionRenewalHistory>()
            .HasMany(p => p.Sellers)
            .WithMany(p => p.SubscriptionRenewalHistories);

    }
    
    private void AddFavoriteConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favorite>()
            .HasOne(p => p.Service)
            .WithMany(p => p.Favorites)
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    private void AddSubscriptionCampaignConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionCampaign>()
            .HasOne(p => p.SubscriptionPlan)
            .WithMany(p => p.SubscriptionCampaigns)
            .HasForeignKey(p => p.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void AddSellerSubscriptionConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SellerSubscription>()
            .HasOne(p => p.Seller)
            .WithOne(p => p.SellerSubscription)
            .HasForeignKey<SellerSubscription>(p => p.SellerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SellerSubscription>()
            .HasOne(p => p.SubscriptionPlan)
            .WithMany(p => p.SellerSubscriptions)
            .HasForeignKey(p => p.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    private void AddServiceConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>()
            .HasOne(p => p.Seller)
            .WithMany(p => p.Services)
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // modelBuilder.Entity<Service>()
        //     .HasOne(p => p.SubCategory)
        //     .WithMany(p => p.Services)
        //     .HasForeignKey(p => p.SubCategoryId)
        //     .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Service>()
            .HasOne(p => p.Category)
            .WithMany(p => p.Services)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    private void AddCampaignUsageConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CampaignUsage>()
            .HasOne(u => u.Seller)
            .WithMany(u => u.CampaignUsages)
            .HasForeignKey(u => u.SellerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CampaignUsage>()
            .HasOne(u => u.SubscriptionCampaign)
            .WithMany()
            .HasForeignKey(u => u.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

    }
    private void AddSubCategoryConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // Silerken döngü hatasını engeller

        // modelBuilder.Entity<SubCategory>()
        //     .HasOne(p => p.Category)
        //     .WithMany(p => p.SubCategories)
        //     .HasForeignKey(p => p.CategoryId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
    private void AddServiceViewLogConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceViewLog>()
            .HasOne(p => p.Service)
            .WithMany(p => p.ServiceViewLogs)
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    private void AddSellerInfoConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Seller>()
            .HasOne(p => p.SellerInfo)
            .WithOne(p => p.Seller)
            .HasForeignKey<SellerInfo>(p => p.SellerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    private void AddMessageConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Service)
            .WithMany()
            .HasForeignKey(m => m.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.ReceiverId, m.IsRead });
        
        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.ServiceId, m.SenderId, m.ReceiverId });
    }
    
    private void AddLocationConfig(ModelBuilder modelBuilder)
    {
        // City
        modelBuilder.Entity<City>()
            .HasIndex(c => c.Name)
            .IsUnique();
        
        modelBuilder.Entity<City>()
            .HasIndex(c => c.PlateCode)
            .IsUnique();
        
        // District -> City relationship
        modelBuilder.Entity<District>()
            .HasOne(d => d.City)
            .WithMany(c => c.Districts)
            .HasForeignKey(d => d.CityId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<District>()
            .HasIndex(d => new { d.CityId, d.Name })
            .IsUnique();
        
        // Neighborhood -> District relationship
        modelBuilder.Entity<Neighborhood>()
            .HasOne(n => n.District)
            .WithMany(d => d.Neighborhoods)
            .HasForeignKey(n => n.DistrictId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Neighborhood>()
            .HasIndex(n => new { n.DistrictId, n.Name });
    }
    
    public void SetAuditFields()
    {
        var entries = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedDate = DateTime.Now;
            }
        }
    }
}