using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetSubscriptionPlanListQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SubscriptionPlan;

public class GetSubscriptionPlanListQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSubscriptionPlan>> _planRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSellerSubscription>> _subscriptionRepositoryMock;
    private readonly GetSubscriptionPlanListQueryHandler _handler;

    public GetSubscriptionPlanListQueryHandlerTests()
    {
        _planRepositoryMock = new Mock<IGenericRepository<DomainSubscriptionPlan>>();
        _subscriptionRepositoryMock = new Mock<IGenericRepository<DomainSellerSubscription>>();
        _handler = new GetSubscriptionPlanListQueryHandler(_planRepositoryMock.Object, _subscriptionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Active_Plans()
    {
        var sellerId = Guid.NewGuid();
        _subscriptionRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSellerSubscription>().AsQueryable().BuildMock());
        _planRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSubscriptionPlan>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSubscriptionPlanListQuery { SellerId = sellerId }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.SubscriptionPlanList.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Active_Plans_When_No_Current_Subscription()
    {
        var sellerId = Guid.NewGuid();
        var plans = new List<DomainSubscriptionPlan>
        {
            CreatePlan("Free", SubscriptionType.Free, 0),
            CreatePlan("Standard", SubscriptionType.Standard, 29900),
            CreatePlan("Premium", SubscriptionType.Premium, 49900)
        };

        _subscriptionRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSellerSubscription>().AsQueryable().BuildMock());
        _planRepositoryMock.Setup(r => r.GetAllQuery()).Returns(plans.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSubscriptionPlanListQuery { SellerId = sellerId }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.SubscriptionPlanList.Should().HaveCount(3);
        result.SubscriptionPlanList.Select(p => p.Name).Should().ContainInOrder("Free", "Standard", "Premium");
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Upgrade_Plans_When_Current_Subscription_Exists()
    {
        var sellerId = Guid.NewGuid();
        var standardPlanId = Guid.NewGuid();
        var standardPlan = CreatePlan("Standard", SubscriptionType.Standard, 29900, standardPlanId);
        var premiumPlan = CreatePlan("Premium", SubscriptionType.Premium, 49900);
        var corporatePlan = CreatePlan("Corporate", SubscriptionType.Corporate, 99900);
        var freePlan = CreatePlan("Free", SubscriptionType.Free, 0);

        var currentSubscription = new DomainSellerSubscription
        {
            SellerId = sellerId,
            SubscriptionPlanId = standardPlanId,
            SubscriptionPlan = standardPlan,
            IsActive = true
        };

        _subscriptionRepositoryMock.Setup(r => r.GetAllQuery())
            .Returns(new List<DomainSellerSubscription> { currentSubscription }.AsQueryable().BuildMock());
        _planRepositoryMock.Setup(r => r.GetAllQuery())
            .Returns(new List<DomainSubscriptionPlan> { freePlan, standardPlan, premiumPlan, corporatePlan }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSubscriptionPlanListQuery { SellerId = sellerId }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.SubscriptionPlanList.Should().HaveCount(2);
        result.SubscriptionPlanList.Select(p => p.Name).Should().ContainInOrder("Premium", "Corporate");
        result.SubscriptionPlanList.Should().NotContain(p => p.Name == "Standard" || p.Name == "Free");
    }

    private static DomainSubscriptionPlan CreatePlan(string name, SubscriptionType type, int price, Guid? id = null)
    {
        return new DomainSubscriptionPlan
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = $"{name} plan",
            SubscriptionType = type,
            MonthlyPrice = price,
            MonthlyPostLimit = 10,
            PostDurationInDays = 30,
            FeaturedPostLimit = 1,
            IsActive = true
        };
    }
}
