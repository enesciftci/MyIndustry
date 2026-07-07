using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminListingsQuery;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminStatsQuery;
using MyIndustry.ApplicationService.Handler.Admin.SuspendListingCommand;
using MyIndustry.Domain.Aggregate;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.Admin;

public class AdminIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<DomainSeller> _sellerRepository;
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Message> _messageRepository;
    private readonly IGenericRepository<DomainCategory> _categoryRepository;
    private readonly IGenericRepository<Domain.Aggregate.SellerSubscription> _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _sellerRepository = new GenericRepository<DomainSeller>(_context);
        _serviceRepository = new GenericRepository<Domain.Aggregate.Service>(_context);
        _subscriptionRepository = new GenericRepository<Domain.Aggregate.SellerSubscription>(_context);
        _messageRepository = new GenericRepository<Domain.Aggregate.Message>(_context);
        _categoryRepository = new GenericRepository<DomainCategory>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task GetAdminStats_Should_Return_Aggregated_Counts()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Pending Listing");
        await TestDataBuilder.SeedCategoryAsync(_context, "Stats Category");

        var handler = new GetAdminStatsQueryHandler(
            _sellerRepository, _serviceRepository, _messageRepository, _categoryRepository);
        var result = await handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Stats.Should().NotBeNull();
        result.Stats!.TotalSellers.Should().BeGreaterThan(0);
        result.Stats.TotalListings.Should().BeGreaterThan(0);
        result.Stats.TotalCategories.Should().BeGreaterThan(0);
        result.Stats.RecentActivities.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAdminListings_Pending_Should_Filter_Unapproved()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var pending = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id, "Pending");
        pending.IsApproved = false;
        _context.Services.Update(pending);
        await _context.SaveChangesAsync();

        var handler = new GetAdminListingsQueryHandler(_serviceRepository);
        var result = await handler.Handle(new GetAdminListingsQuery { Status = "pending" }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Listings.Should().OnlyContain(l => l.Status == "pending");
        result.Listings.Should().Contain(l => l.Title == "Pending");
    }

    [Fact]
    public async Task ApproveListing_Should_Set_IsApproved()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);
        service.IsApproved = false;
        _context.Services.Update(service);
        await _context.SaveChangesAsync();

        var handler = new ApproveListingCommandHandler(_serviceRepository, _subscriptionRepository, _unitOfWork);
        var result = await handler.Handle(new ApproveListingCommand
        {
            ServiceId = service.Id,
            Approve = true
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var approved = await _context.Services.FindAsync(service.Id);
        approved!.IsApproved.Should().BeTrue();
        approved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SuspendListing_Should_Deactivate_Service()
    {
        var (seller, _, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);

        var handler = new SuspendListingCommandHandler(_serviceRepository, _unitOfWork);
        var result = await handler.Handle(new SuspendListingCommand
        {
            ServiceId = service.Id,
            Suspend = true,
            SuspensionReasonDescription = "Policy violation"
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var suspended = await _context.Services.FindAsync(service.Id);
        suspended!.IsActive.Should().BeFalse();
        suspended.SuspensionReasonDescription.Should().Be("Policy violation");
    }

    [Fact]
    public async Task RejectListing_Should_Deactivate_And_Restore_Quota()
    {
        var (seller, subscription, category, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var service = await TestDataBuilder.SeedServiceAsync(_context, seller.Id, category.Id);
        service.IsApproved = false;
        _context.Services.Update(service);
        subscription.RemainingPostQuota = 2;
        _context.SellerSubscriptions.Update(subscription);
        await _context.SaveChangesAsync();

        var handler = new ApproveListingCommandHandler(_serviceRepository, _subscriptionRepository, _unitOfWork);
        var result = await handler.Handle(new ApproveListingCommand
        {
            ServiceId = service.Id,
            Approve = false,
            RejectionReasonDescription = "Incomplete listing"
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var rejected = await _context.Services.FindAsync(service.Id);
        rejected!.IsApproved.Should().BeFalse();
        rejected.IsActive.Should().BeFalse();

        var updatedSub = await _context.SellerSubscriptions.FindAsync(subscription.Id);
        updatedSub!.RemainingPostQuota.Should().Be(3);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
