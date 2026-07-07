using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.Service.GetServiceBySlugQuery;
using MyIndustry.Domain.Aggregate;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Integration.SEO;

public class SEOIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<DomainCategory> _categoryRepository;
    private readonly IGenericRepository<DomainSeller> _sellerRepository;

    public SEOIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _serviceRepository = new GenericRepository<Domain.Aggregate.Service>(_context);
        _categoryRepository = new GenericRepository<DomainCategory>(_context);
        _sellerRepository = new GenericRepository<DomainSeller>(_context);
    }

    [Fact]
    public async Task GetServiceBySlug_Should_Resolve_Service()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "SEO Test Machine");
        service.Slug = "seo-test-machine";
        _context.Services.Update(service);
        await _context.SaveChangesAsync();

        var handler = new GetServiceBySlugQueryHandler(_serviceRepository, _categoryRepository, _sellerRepository);
        var result = await handler.Handle(
            new GetServiceBySlugQuery { Slug = "seo-test-machine" },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Service!.Id.Should().Be(service.Id);
        result.Service.Title.Should().Be("SEO Test Machine");
    }

    [Fact]
    public async Task GetServiceBySlug_Should_Fallback_To_Id()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);

        var handler = new GetServiceBySlugQueryHandler(_serviceRepository, _categoryRepository, _sellerRepository);
        var result = await handler.Handle(
            new GetServiceBySlugQuery { Slug = service.Id.ToString() },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Service!.Id.Should().Be(service.Id);
    }

    [Fact]
    public async Task ActiveServices_Should_Have_Slugs_For_Sitemap()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Sitemap Service 1");
        await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Sitemap Service 2");

        var services = await _serviceRepository
            .GetAllQuery()
            .Where(s => s.IsActive && s.IsApproved)
            .Select(s => new { s.Id, s.Slug })
            .ToListAsync();

        services.Should().HaveCount(2);
        services.Should().OnlyContain(s => !string.IsNullOrEmpty(s.Slug));
    }

    [Fact]
    public async Task ActiveCategories_Should_Have_Slugs_For_Sitemap()
    {
        var category = await TestDataBuilder.SeedCategoryAsync(_context, "SEO Category");
        category.Slug = "seo-category";
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        var categories = await _categoryRepository
            .GetAllQuery()
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.Slug })
            .ToListAsync();

        categories.Should().Contain(c => c.Slug == "seo-category");
    }

    [Fact]
    public async Task SeededService_Should_Have_Meta_Fields()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Meta Title Service");
        service.MetaTitle = "Meta Title Service - Satılık | MyIndustry";
        service.MetaDescription = "Industrial equipment listing";
        service.MetaKeywords = "meta, service, myindustry";
        _context.Services.Update(service);
        await _context.SaveChangesAsync();

        var saved = await _context.Services.FindAsync(service.Id);
        saved!.MetaTitle.Should().Contain("Meta Title Service");
        saved.MetaDescription.Should().NotBeNullOrEmpty();
        saved.MetaKeywords.Should().Contain("myindustry");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
