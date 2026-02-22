using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.UpdateSubscriptionPlanCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SubscriptionPlan;

public class UpdateSubscriptionPlanCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSubscriptionPlan>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateSubscriptionPlanCommandHandler _handler;

    public UpdateSubscriptionPlanCommandHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<DomainSubscriptionPlan>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateSubscriptionPlanCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_SubscriptionPlan_Successfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new DomainSubscriptionPlan
        {
            Id = planId,
            Name = "Old Plan",
            Description = "Old Description",
            SubscriptionType = SubscriptionType.Free,
            MonthlyPrice = 0,
            MonthlyPostLimit = 3,
            PostDurationInDays = 30,
            FeaturedPostLimit = 0,
            IsActive = true
        };

        var command = new UpdateSubscriptionPlanCommand
        {
            Id = planId,
            Name = "Updated Plan",
            Description = "Updated Description",
            SubscriptionType = SubscriptionType.Premium,
            MonthlyPrice = 59900,
            MonthlyPostLimit = 50,
            PostDurationInDays = 60,
            FeaturedPostLimit = 10,
            IsActive = true
        };

        _repositoryMock
            .Setup(r => r.GetById(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        existingPlan.Name.Should().Be(command.Name);
        existingPlan.Description.Should().Be(command.Description);
        existingPlan.SubscriptionType.Should().Be(command.SubscriptionType);
        existingPlan.MonthlyPrice.Should().Be(command.MonthlyPrice);
        existingPlan.MonthlyPostLimit.Should().Be(command.MonthlyPostLimit);
        existingPlan.PostDurationInDays.Should().Be(command.PostDurationInDays);
        existingPlan.FeaturedPostLimit.Should().Be(command.FeaturedPostLimit);
        existingPlan.IsActive.Should().Be(command.IsActive);
        existingPlan.ModifiedDate.Should().NotBeNull();
        _repositoryMock.Verify(r => r.Update(existingPlan), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Plan_Not_Found()
    {
        // Arrange
        var command = new UpdateSubscriptionPlanCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Plan",
            Description = "Test Description",
            SubscriptionType = SubscriptionType.Standard,
            MonthlyPrice = 29900,
            MonthlyPostLimit = 15,
            PostDurationInDays = 45,
            FeaturedPostLimit = 2,
            IsActive = true
        };

        _repositoryMock
            .Setup(r => r.GetById(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscriptionPlan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
