using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.GetAllSubscriptionPlansQuery;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SubscriptionPlan;

public class GetAllSubscriptionPlansQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSubscriptionPlan>> _repositoryMock;
    private readonly GetAllSubscriptionPlansQueryHandler _handler;

    public GetAllSubscriptionPlansQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<DomainSubscriptionPlan>>();
        _handler = new GetAllSubscriptionPlansQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Plans()
    {
        _repositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSubscriptionPlan>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAllSubscriptionPlansQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Plans.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Plans_Ordered_By_SubscriptionType()
    {
        var plans = new List<DomainSubscriptionPlan>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Premium",
                Description = "Premium plan",
                SubscriptionType = SubscriptionType.Premium,
                MonthlyPrice = 49900,
                MonthlyPostLimit = 50,
                PostDurationInDays = 60,
                FeaturedPostLimit = 5,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Free",
                Description = "Free plan",
                SubscriptionType = SubscriptionType.Free,
                MonthlyPrice = 0,
                MonthlyPostLimit = 3,
                PostDurationInDays = 15,
                FeaturedPostLimit = 0,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                Description = "Standard plan",
                SubscriptionType = SubscriptionType.Standard,
                MonthlyPrice = 29900,
                MonthlyPostLimit = 15,
                PostDurationInDays = 30,
                FeaturedPostLimit = 2,
                IsActive = false
            }
        };

        _repositoryMock.Setup(r => r.GetAllQuery()).Returns(plans.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAllSubscriptionPlansQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Plans.Should().HaveCount(3);
        result.Plans.Select(p => p.SubscriptionType).Should().ContainInOrder("Free", "Standard", "Premium");
        result.Plans.Single(p => p.Name == "Standard").IsActive.Should().BeFalse();
    }
}
