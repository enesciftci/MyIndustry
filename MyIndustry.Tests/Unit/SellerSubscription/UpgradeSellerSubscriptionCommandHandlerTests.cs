using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SellerSubscription.UpgradeSellerSubscriptionCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SellerSubscription;

public class UpgradeSellerSubscriptionCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSubscriptionPlan>> _subscriptionPlanRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSellerSubscription>> _sellerSubscriptionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpgradeSellerSubscriptionCommandHandler _handler;

    public UpgradeSellerSubscriptionCommandHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _subscriptionPlanRepositoryMock = new Mock<IGenericRepository<DomainSubscriptionPlan>>();
        _sellerSubscriptionRepositoryMock = new Mock<IGenericRepository<DomainSellerSubscription>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpgradeSellerSubscriptionCommandHandler(
            _sellerRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _sellerSubscriptionRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Upgrade_When_Current_Subscription_Exists_And_New_Plan_Different()
    {
        var sellerId = Guid.NewGuid();
        var currentPlanId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId };
        var newPlan = new DomainSubscriptionPlan
        {
            Id = newPlanId,
            IsActive = true,
            PostDurationInDays = 30,
            MonthlyPostLimit = 20,
            FeaturedPostLimit = 5
        };
        var currentSub = new DomainSellerSubscription
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            SubscriptionPlanId = currentPlanId,
            IsActive = true
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _subscriptionPlanRepositoryMock.Setup(r => r.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<DomainSubscriptionPlan, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(newPlan);
        _sellerSubscriptionRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSellerSubscription> { currentSub }.AsQueryable().BuildMock());
        _sellerSubscriptionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainSellerSubscription>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new UpgradeSellerSubscriptionCommand
        {
            SellerId = sellerId,
            SubscriptionPlanId = newPlanId,
            IsAutoRenew = true
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        currentSub.IsActive.Should().BeFalse();
        _sellerSubscriptionRepositoryMock.Verify(r => r.Update(currentSub), Times.Once);
        _sellerSubscriptionRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainSellerSubscription>(s =>
            s.SellerId == sellerId && s.SubscriptionPlanId == newPlanId && s.IsActive
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Seller_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller>().AsQueryable().BuildMock());

        var command = new UpgradeSellerSubscriptionCommand { SellerId = Guid.NewGuid(), SubscriptionPlanId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Satıcı bulunamadı.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_No_Active_Subscription()
    {
        var sellerId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId };
        var plan = new DomainSubscriptionPlan { Id = Guid.NewGuid(), IsActive = true };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _subscriptionPlanRepositoryMock.Setup(r => r.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<DomainSubscriptionPlan, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sellerSubscriptionRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSellerSubscription>().AsQueryable().BuildMock());

        var command = new UpgradeSellerSubscriptionCommand { SellerId = sellerId, SubscriptionPlanId = plan.Id };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*Aktif bir aboneliğiniz bulunmamaktadır*");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Already_On_Same_Plan()
    {
        var sellerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId };
        var plan = new DomainSubscriptionPlan { Id = planId, IsActive = true };
        var currentSub = new DomainSellerSubscription { SellerId = sellerId, SubscriptionPlanId = planId, IsActive = true };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _subscriptionPlanRepositoryMock.Setup(r => r.GetById(It.IsAny<System.Linq.Expressions.Expression<Func<DomainSubscriptionPlan, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _sellerSubscriptionRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSellerSubscription> { currentSub }.AsQueryable().BuildMock());

        var command = new UpgradeSellerSubscriptionCommand { SellerId = sellerId, SubscriptionPlanId = planId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Zaten bu plana sahipsiniz.");
    }
}
