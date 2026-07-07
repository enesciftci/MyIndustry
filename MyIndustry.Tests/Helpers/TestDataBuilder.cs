using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Helpers;

public static class TestDataBuilder
{
    public static async Task<Category> SeedCategoryAsync(MyIndustryDbContext context, string name = "Test Category")
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test category description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public static async Task<DomainSubscriptionPlan> SeedSubscriptionPlanAsync(
        MyIndustryDbContext context,
        SubscriptionType type = SubscriptionType.Standard,
        string name = "Standard Plan")
    {
        var plan = new DomainSubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test plan",
            SubscriptionType = type,
            MonthlyPrice = 29900,
            MonthlyPostLimit = 10,
            PostDurationInDays = 30,
            FeaturedPostLimit = 2,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.SubscriptionPlans.Add(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public static async Task<Seller> SeedSellerAsync(MyIndustryDbContext context, Guid userId, string title = "Test Seller")
    {
        var seller = new Seller
        {
            Id = userId,
            Title = title,
            Description = "Test seller",
            IdentityNumber = "encrypted",
            AgreementUrl = "http://example.com",
            Sector = SellerSector.IronMongery,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.Sellers.Add(seller);
        await context.SaveChangesAsync();
        return seller;
    }

    public static async Task<Service> SeedServiceAsync(
        MyIndustryDbContext context,
        Guid sellerId,
        Guid categoryId,
        string title = "Test Service")
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            CategoryId = categoryId,
            Title = title,
            Description = "Test description",
            Price = 100,
            EstimatedEndDay = 7,
            ImageUrls = "[]",
            Slug = title.ToLowerInvariant().Replace(' ', '-'),
            IsActive = true,
            IsApproved = true,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();
        return service;
    }

    public static async Task<DomainSubscriptionPlan> SeedFreePlanAsync(MyIndustryDbContext context)
    {
        var plan = new DomainSubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Free Plan",
            Description = "Free tier",
            SubscriptionType = SubscriptionType.Free,
            MonthlyPrice = 0,
            MonthlyPostLimit = 5,
            PostDurationInDays = 30,
            FeaturedPostLimit = 0,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.SubscriptionPlans.Add(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public static async Task<(Seller Seller, SellerSubscription Subscription, Category Category, DomainSubscriptionPlan Plan)>
        SeedSellerWithActiveSubscriptionAsync(MyIndustryDbContext context, Guid? sellerId = null)
    {
        var plan = await SeedFreePlanAsync(context);
        var category = await SeedCategoryAsync(context);
        var userId = sellerId ?? Guid.NewGuid();

        var seller = new Seller
        {
            Id = userId,
            Title = "Integration Seller",
            Description = "Seller for integration tests",
            IdentityNumber = "encrypted-id",
            AgreementUrl = "https://example.com/agreement",
            Sector = SellerSector.IronMongery,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            SellerInfo = new SellerInfo
            {
                Id = Guid.NewGuid(),
                SellerId = userId,
                Email = "seller@integration.test",
                PhoneNumber = "+905551234567"
            }
        };

        var subscription = new SellerSubscription
        {
            Id = Guid.NewGuid(),
            SellerId = userId,
            SubscriptionPlanId = plan.Id,
            StartDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(plan.PostDurationInDays),
            RemainingPostQuota = plan.MonthlyPostLimit,
            RemainingFeaturedQuota = plan.FeaturedPostLimit,
            IsAutoRenew = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        context.Sellers.Add(seller);
        context.SellerSubscriptions.Add(subscription);
        await context.SaveChangesAsync();
        return (seller, subscription, category, plan);
    }

    public static async Task<LegalDocument> SeedLegalDocumentAsync(
        MyIndustryDbContext context,
        LegalDocumentType type = LegalDocumentType.MembershipAgreement,
        string title = "Test Legal Document")
    {
        var document = new LegalDocument
        {
            Id = Guid.NewGuid(),
            DocumentType = type,
            Title = title,
            Content = "Legal document content for integration tests.",
            Version = "1.0",
            IsActive = true,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            DisplayOrder = 1,
            CreatedDate = DateTime.UtcNow
        };
        context.LegalDocuments.Add(document);
        await context.SaveChangesAsync();
        return document;
    }

    public static async Task<(City City, District District, Neighborhood Neighborhood)> SeedLocationHierarchyAsync(
        MyIndustryDbContext context)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = "İstanbul",
            PlateCode = 34,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var district = new District
        {
            Id = Guid.NewGuid(),
            Name = "Kadıköy",
            CityId = city.Id,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var neighborhood = new Neighborhood
        {
            Id = Guid.NewGuid(),
            Name = "Moda",
            DistrictId = district.Id,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        context.Cities.Add(city);
        context.Districts.Add(district);
        context.Neighborhoods.Add(neighborhood);
        await context.SaveChangesAsync();
        return (city, district, neighborhood);
    }
}
