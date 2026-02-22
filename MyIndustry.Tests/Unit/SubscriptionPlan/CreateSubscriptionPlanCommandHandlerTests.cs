using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.CreateSubscriptionPlanCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SubscriptionPlan;

public class CreateSubscriptionPlanCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSubscriptionPlan>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateSubscriptionPlanCommandHandler _handler;

    public CreateSubscriptionPlanCommandHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<DomainSubscriptionPlan>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateSubscriptionPlanCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_SubscriptionPlan_Successfully()
    {
        // Arrange
        var command = new CreateSubscriptionPlanCommand
        {
            Name = "Test Plan",
            Description = "Test Description",
            SubscriptionType = SubscriptionType.Standard,
            MonthlyPrice = 29900,
            MonthlyPostLimit = 15,
            PostDurationInDays = 45,
            FeaturedPostLimit = 2
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<DomainSubscriptionPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddAsync(It.Is<DomainSubscriptionPlan>(p =>
            p.Name == command.Name &&
            p.Description == command.Description &&
            p.SubscriptionType == command.SubscriptionType &&
            p.MonthlyPrice == command.MonthlyPrice &&
            p.MonthlyPostLimit == command.MonthlyPostLimit &&
            p.PostDurationInDays == command.PostDurationInDays &&
            p.FeaturedPostLimit == command.FeaturedPostLimit &&
            p.IsActive == true
        ), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
