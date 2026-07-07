using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;
using MyIndustry.ApplicationService.Handler.Service.DeleteServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;
using MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using DomainService = MyIndustry.Domain.Aggregate.Service;
using SubCategory = MyIndustry.Domain.Aggregate.SubCategory;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.Service;

public class ServiceIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IGenericRepository<DomainSeller> _sellerRepository;
    private readonly IGenericRepository<DomainCategory> _categoryRepository;
    private readonly IGenericRepository<SubCategory> _subCategoryRepository;
    private readonly IGenericRepository<DomainSellerSubscription> _sellerSubscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _serviceRepository = new GenericRepository<DomainService>(_context);
        _sellerRepository = new GenericRepository<DomainSeller>(_context);
        _categoryRepository = new GenericRepository<DomainCategory>(_context);
        _subCategoryRepository = new GenericRepository<SubCategory>(_context);
        _sellerSubscriptionRepository = new GenericRepository<DomainSellerSubscription>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task CreateService_Should_Save_To_Database_And_Decrease_Quota()
    {
        var (seller, subscription, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var initialQuota = subscription.RemainingPostQuota;

        var handler = new CreateServiceCommandHandler(
            _serviceRepository, _unitOfWork, _sellerRepository, _subCategoryRepository,
            _categoryRepository, _sellerSubscriptionRepository);

        var command = new CreateServiceCommand
        {
            SellerId = seller.Id,
            CategoryId = category.Id,
            Title = "Integration CNC Machine",
            Description = "High precision CNC machine for industrial use",
            Price = 150000,
            EstimatedEndDay = 14,
            ImageUrls = "[]",
            City = "İstanbul",
            ListingType = ListingType.ForSale
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        var saved = await _context.Services.FirstOrDefaultAsync(s => s.Title == command.Title);
        saved.Should().NotBeNull();
        saved!.SellerId.Should().Be(seller.Id);
        saved.CategoryId.Should().Be(category.Id);
        saved.Slug.Should().NotBeNullOrEmpty();
        saved.MetaTitle.Should().Contain(command.Title);

        var updatedSub = await _context.SellerSubscriptions.FindAsync(subscription.Id);
        updatedSub!.RemainingPostQuota.Should().Be(initialQuota - 1);
    }

    [Fact]
    public async Task GetServiceById_Should_Return_Service_With_Breadcrumbs()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Get By Id Service");

        var handler = new GetServicesByIdQueryHandler(_serviceRepository, _categoryRepository, _sellerRepository);
        var result = await handler.Handle(new GetServicesByIdQuery { Id = service.Id }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Service.Should().NotBeNull();
        result.Service!.Id.Should().Be(service.Id);
        result.Service.Title.Should().Be(service.Title);
        result.Service.CategoryBreadcrumbs.Should().Contain(b => b.Name == category.Name);
        result.Service.Seller.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateService_Should_Persist_Changes()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Original Title");

        var handler = new UpdateServiceByIdCommandHandler(
            _serviceRepository, _sellerRepository, _categoryRepository, _unitOfWork);

        var command = new UpdateServiceByIdCommand
        {
            ServiceDto = new ServiceDto
            {
                Id = service.Id,
                SellerId = seller.Id,
                CategoryId = category.Id,
                Title = "Updated Title",
                Description = "Updated description",
                Price = 200000,
                EstimatedEndDay = 21,
                ImageUrls = Array.Empty<string>(),
                IsFeatured = false,
                ListingType = 0
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        var updated = await _context.Services.FindAsync(service.Id);
        updated!.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(200000);
        updated.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteService_Should_Remove_From_Database()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Delete Me");

        var handler = new DeleteServiceByIdCommandHandler(_unitOfWork, _serviceRepository);
        var result = await handler.Handle(
            new DeleteServiceByIdCommand { Id = service.Id, SellerId = seller.Id },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        var deleted = await _context.Services.FindAsync(service.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task SearchServices_Should_Return_Matching_Approved_Listings()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Hydraulic Press Machine");
        await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Unrelated Item");

        var handler = new GetServicesBySearchTermQueryHandler(_serviceRepository);
        var result = await handler.Handle(
            new GetServicesBySearchTermQuery { Query = "hydraulic", Pager = new Pager(1, 10) },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Services.Should().HaveCount(1);
        result.Services[0].Title.Should().Contain("Hydraulic");
        result.TotalCount.Should().Be(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
