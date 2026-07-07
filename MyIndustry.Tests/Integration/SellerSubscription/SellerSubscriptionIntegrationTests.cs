using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.SellerSubscription.CreateSellerSubscriptionCommand;
using MyIndustry.ApplicationService.Handler.SellerSubscription.GetSellerSubscriptionQuery;
using MyIndustry.ApplicationService.Handler.SellerSubscription.UpgradeSellerSubscriptionCommand;
using MyIndustry.Domain.Aggregate;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.SellerSubscription;

public class SellerSubscriptionIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<DomainSeller> _sellerRepository;
    private readonly IGenericRepository<DomainSellerSubscription> _sellerSubscriptionRepository;
    private readonly IGenericRepository<DomainSubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SellerSubscriptionIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _sellerRepository = new GenericRepository<DomainSeller>(_context);
        _sellerSubscriptionRepository = new GenericRepository<DomainSellerSubscription>(_context);
        _subscriptionPlanRepository = new GenericRepository<DomainSubscriptionPlan>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task CreateSellerSubscription_Should_Save_Active_Subscription()
    {
        var plan = await TestDataBuilder.SeedSubscriptionPlanAsync(_context, SubscriptionType.Standard, "Standard");
        var sellerId = Guid.NewGuid();
        _context.Sellers.Add(new DomainSeller
        {
            Id = sellerId,
            Title = "No Sub Seller",
            Description = "Test",
            IdentityNumber = "enc",
            AgreementUrl = "https://example.com",
            Sector = SellerSector.IronMongery,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new CreateSellerSubscriptionCommandHandler(
            _sellerRepository, _unitOfWork, _subscriptionPlanRepository, _sellerSubscriptionRepository);

        var result = await handler.Handle(new CreateSellerSubscriptionCommand
        {
            SellerId = sellerId,
            SubscriptionPlanId = plan.Id,
            IsAutoRenew = true
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var subscription = await _context.SellerSubscriptions
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && s.IsActive);
        subscription.Should().NotBeNull();
        subscription!.RemainingPostQuota.Should().Be(plan.MonthlyPostLimit);
        subscription.RemainingFeaturedQuota.Should().Be(plan.FeaturedPostLimit);
    }

    [Fact]
    public async Task GetSellerSubscription_Should_Return_Active_Plan()
    {
        var (seller, subscription, _, plan) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);

        var handler = new GetSellerSubscriptionQueryHandler(_sellerSubscriptionRepository);
        var result = await handler.Handle(
            new GetSellerSubscriptionQuery { SellerId = seller.Id },
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.SellerSubscriptionDto.Should().NotBeNull();
        result.SellerSubscriptionDto!.Name.Should().Be(plan.Name);
        result.SellerSubscriptionDto.RemainingPostQuota.Should().Be(subscription.RemainingPostQuota);
    }

    [Fact]
    public async Task UpgradeSellerSubscription_Should_Deactivate_Old_And_Create_New()
    {
        var (seller, oldSub, _, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);
        var premiumPlan = await TestDataBuilder.SeedSubscriptionPlanAsync(
            _context, SubscriptionType.Premium, "Premium Plan");

        var handler = new UpgradeSellerSubscriptionCommandHandler(
            _sellerRepository, _subscriptionPlanRepository, _sellerSubscriptionRepository, _unitOfWork);

        var result = await handler.Handle(new UpgradeSellerSubscriptionCommand
        {
            SellerId = seller.Id,
            SubscriptionPlanId = premiumPlan.Id,
            IsAutoRenew = true
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var oldSubscription = await _context.SellerSubscriptions.FindAsync(oldSub.Id);
        oldSubscription!.IsActive.Should().BeFalse();

        var newSubscription = await _context.SellerSubscriptions
            .FirstOrDefaultAsync(s => s.SellerId == seller.Id && s.IsActive);
        newSubscription.Should().NotBeNull();
        newSubscription!.SubscriptionPlanId.Should().Be(premiumPlan.Id);
        newSubscription.RemainingPostQuota.Should().Be(premiumPlan.MonthlyPostLimit);
    }

    [Fact]
    public async Task CreateSellerSubscription_When_Active_Exists_Should_Throw()
    {
        var (seller, _, _, plan) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);

        var handler = new CreateSellerSubscriptionCommandHandler(
            _sellerRepository, _unitOfWork, _subscriptionPlanRepository, _sellerSubscriptionRepository);

        var act = () => handler.Handle(new CreateSellerSubscriptionCommand
        {
            SellerId = seller.Id,
            SubscriptionPlanId = plan.Id,
            IsAutoRenew = true
        }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
