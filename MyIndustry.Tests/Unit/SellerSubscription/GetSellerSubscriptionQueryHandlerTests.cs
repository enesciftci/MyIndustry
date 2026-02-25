using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SellerSubscription.GetSellerSubscriptionQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SellerSubscription;

public class GetSellerSubscriptionQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSellerSubscription>> _sellerSubscriptionRepositoryMock;
    private readonly GetSellerSubscriptionQueryHandler _handler;

    public GetSellerSubscriptionQueryHandlerTests()
    {
        _sellerSubscriptionRepositoryMock = new Mock<IGenericRepository<DomainSellerSubscription>>();
        _handler = new GetSellerSubscriptionQueryHandler(_sellerSubscriptionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Subscription_When_Active_Exists()
    {
        var sellerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var plan = new DomainSubscriptionPlan { Id = planId, Name = "Premium" };
        var sub = new DomainSellerSubscription
        {
            SellerId = sellerId,
            SubscriptionPlanId = planId,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-10),
            ExpiryDate = DateTime.UtcNow.AddDays(20),
            RemainingPostQuota = 5,
            RemainingFeaturedQuota = 1,
            SubscriptionPlan = plan
        };
        _sellerSubscriptionRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSellerSubscription> { sub }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerSubscriptionQuery { SellerId = sellerId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.SellerSubscriptionDto.Should().NotBeNull();
        result.SellerSubscriptionDto!.Name.Should().Be("Premium");
        result.SellerSubscriptionDto.RemainingPostQuota.Should().Be(5);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_No_Active_Subscription()
    {
        _sellerSubscriptionRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSellerSubscription>().AsQueryable().BuildMock());

        var act = () => _handler.Handle(new GetSellerSubscriptionQuery { SellerId = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Abonelik bulunamadı.");
    }
}
