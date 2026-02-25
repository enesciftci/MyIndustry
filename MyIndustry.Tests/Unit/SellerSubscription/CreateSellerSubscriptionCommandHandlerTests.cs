using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SellerSubscription.CreateSellerSubscriptionCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SellerSubscription;

public class CreateSellerSubscriptionCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSellerSubscription>> _sellerSubscriptionRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSubscriptionPlan>> _subscriptionPlanRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateSellerSubscriptionCommandHandler _handler;

    public CreateSellerSubscriptionCommandHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _sellerSubscriptionRepositoryMock = new Mock<IGenericRepository<DomainSellerSubscription>>();
        _subscriptionPlanRepositoryMock = new Mock<IGenericRepository<DomainSubscriptionPlan>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateSellerSubscriptionCommandHandler(
            _sellerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _sellerSubscriptionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Subscription_When_Seller_Has_No_Active_Subscription()
    {
        var sellerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId, SellerSubscriptions = new List<DomainSellerSubscription>() };
        var plan = new DomainSubscriptionPlan
        {
            Id = planId,
            IsActive = true,
            PostDurationInDays = 30,
            MonthlyPostLimit = 10,
            FeaturedPostLimit = 2
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _subscriptionPlanRepositoryMock.Setup(r => r.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<DomainSubscriptionPlan, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sellerSubscriptionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainSellerSubscription>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new CreateSellerSubscriptionCommand
        {
            SellerId = sellerId,
            SubscriptionPlanId = planId,
            IsAutoRenew = true
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _sellerSubscriptionRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainSellerSubscription>(s =>
            s.SellerId == sellerId &&
            s.SubscriptionPlanId == planId &&
            s.IsActive &&
            s.RemainingPostQuota == 10
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Seller_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller>().AsQueryable().BuildMock());

        var command = new CreateSellerSubscriptionCommand { SellerId = Guid.NewGuid(), SubscriptionPlanId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Satıcı bulunamadı.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Seller_Already_Has_Active_Subscription()
    {
        var sellerId = Guid.NewGuid();
        var seller = new DomainSeller
        {
            Id = sellerId,
            SellerSubscriptions = new List<DomainSellerSubscription> { new() { IsActive = true } }
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());

        var command = new CreateSellerSubscriptionCommand { SellerId = sellerId, SubscriptionPlanId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Satıcı aboneliği bulunuyor.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Subscription_Plan_Not_Found()
    {
        var sellerId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId, SellerSubscriptions = new List<DomainSellerSubscription>() };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _subscriptionPlanRepositoryMock.Setup(r => r.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<DomainSubscriptionPlan, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((DomainSubscriptionPlan?)null);

        var command = new CreateSellerSubscriptionCommand { SellerId = sellerId, SubscriptionPlanId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Abonelik planı bulunamadı.");
    }
}
