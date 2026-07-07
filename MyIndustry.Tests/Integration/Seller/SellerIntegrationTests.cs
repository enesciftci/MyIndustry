using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerByIdQuery;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerCommand;
using MyIndustry.Domain.Aggregate;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using MyIndustry.Domain.Provider;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.Seller;

public class SellerIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<DomainSeller> _sellerRepository;
    private readonly IGenericRepository<DomainSellerSubscription> _sellerSubscriptionRepository;
    private readonly IGenericRepository<DomainSubscriptionPlan> _subscriptionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityProvider _securityProvider;

    public SellerIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _sellerRepository = new GenericRepository<DomainSeller>(_context);
        _sellerSubscriptionRepository = new GenericRepository<DomainSellerSubscription>(_context);
        _subscriptionPlanRepository = new GenericRepository<DomainSubscriptionPlan>(_context);
        _unitOfWork = new UnitOfWork(_context);
        _securityProvider = new SecurityProvider();
    }

    [Fact]
    public async Task CreateSeller_Should_Save_Seller_And_Free_Subscription()
    {
        await TestDataBuilder.SeedFreePlanAsync(_context);
        var userId = Guid.NewGuid();

        var handler = new CreateSellerCommandHandler(
            _sellerRepository, _sellerSubscriptionRepository, _subscriptionPlanRepository,
            _unitOfWork, _securityProvider);

        var result = await handler.Handle(new CreateSellerCommand
        {
            UserId = userId,
            Title = "New Seller Co",
            Description = "Industrial equipment seller",
            IdentityNumber = "12345678901",
            Email = "newseller@example.com",
            PhoneNumber = "+905551112233",
            AgreementUrl = "https://example.com/seller-agreement",
            Sector = SellerSector.IronMongery
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var seller = await _context.Sellers
            .Include(s => s.SellerSubscriptions)
            .FirstOrDefaultAsync(s => s.Id == userId);
        seller.Should().NotBeNull();
        seller!.Title.Should().Be("New Seller Co");
        seller.SellerSubscriptions.Should().ContainSingle(s => s.IsActive);
    }

    [Fact]
    public async Task GetSellerById_Should_Return_Seller_With_Subscription()
    {
        var (seller, subscription, _, plan) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);

        var handler = new GetSellerByIdQueryHandler(_sellerRepository);
        var result = await handler.Handle(new GetSellerByIdQuery { SellerId = seller.Id }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Seller.Should().NotBeNull();
        result.Seller!.Id.Should().Be(seller.Id);
        result.Seller.Title.Should().Be(seller.Title);
        result.Seller.SellerSubscriptionDto.Should().NotBeNull();
        result.Seller.SellerSubscriptionDto!.Name.Should().Be(plan.Name);
    }

    [Fact]
    public async Task UpdateSeller_Should_Persist_Profile_Changes()
    {
        var (seller, _, _, _) = await TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(_context);

        var handler = new UpdateSellerCommandHandler(_sellerRepository, _unitOfWork, _securityProvider);
        var result = await handler.Handle(new UpdateSellerCommand
        {
            Id = seller.Id,
            Title = "Updated Seller Title",
            Description = "Updated description",
            IdentityNumber = "98765432109",
            Email = "updated@example.com",
            PhoneNumber = "+905559998877",
            AgreementUrl = "https://example.com/updated-agreement",
            Sector = SellerSector.Dumper
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var updated = await _context.Sellers
            .Include(s => s.SellerInfo)
            .FirstOrDefaultAsync(s => s.Id == seller.Id);
        updated!.Title.Should().Be("Updated Seller Title");
        updated.SellerInfo.Email.Should().Be("updated@example.com");
        updated.Sector.Should().Be(SellerSector.Dumper);
    }

    [Fact]
    public async Task CreateSeller_With_Existing_Id_Without_Subscription_Should_Add_Free_Plan()
    {
        var plan = await TestDataBuilder.SeedFreePlanAsync(_context);
        var userId = Guid.NewGuid();
        _context.Sellers.Add(new DomainSeller
        {
            Id = userId,
            Title = "Existing Seller",
            Description = "No subscription yet",
            IdentityNumber = "encrypted",
            AgreementUrl = "https://example.com",
            Sector = SellerSector.IronMongery,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new CreateSellerCommandHandler(
            _sellerRepository, _sellerSubscriptionRepository, _subscriptionPlanRepository,
            _unitOfWork, _securityProvider);

        var result = await handler.Handle(new CreateSellerCommand
        {
            UserId = userId,
            Title = "Existing Seller",
            Description = "No subscription yet",
            IdentityNumber = "12345678901",
            Email = "existing@example.com",
            PhoneNumber = "+905551234567",
            AgreementUrl = "https://example.com",
            Sector = SellerSector.IronMongery
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var subscriptions = await _context.SellerSubscriptions
            .Where(s => s.SellerId == userId && s.IsActive)
            .ToListAsync();
        subscriptions.Should().HaveCount(1);
        subscriptions[0].SubscriptionPlanId.Should().Be(plan.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
