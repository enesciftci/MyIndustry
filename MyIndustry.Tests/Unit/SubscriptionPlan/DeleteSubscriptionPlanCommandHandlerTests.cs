using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.SubscriptionPlan.DeleteSubscriptionPlanCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.SubscriptionPlan;

public class DeleteSubscriptionPlanCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSubscriptionPlan>> _repositoryMock;
    private readonly Mock<IGenericRepository<SellerSubscription>> _sellerSubscriptionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteSubscriptionPlanCommandHandler _handler;

    public DeleteSubscriptionPlanCommandHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<DomainSubscriptionPlan>>();
        _sellerSubscriptionRepositoryMock = new Mock<IGenericRepository<SellerSubscription>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteSubscriptionPlanCommandHandler(
            _repositoryMock.Object,
            _sellerSubscriptionRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_SubscriptionPlan_Successfully()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new DomainSubscriptionPlan
        {
            Id = planId,
            Name = "Test Plan",
            Description = "Test Description",
            SubscriptionType = SubscriptionType.Standard,
            MonthlyPrice = 29900,
            MonthlyPostLimit = 15,
            PostDurationInDays = 45,
            FeaturedPostLimit = 2,
            IsActive = true
        };

        var command = new DeleteSubscriptionPlanCommand { Id = planId };

        _repositoryMock
            .Setup(r => r.GetById(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var mockQueryable = new List<SellerSubscription>().AsQueryable().BuildMock();
        _sellerSubscriptionRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(mockQueryable);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(existingPlan), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Plan_Not_Found()
    {
        // Arrange
        var command = new DeleteSubscriptionPlanCommand { Id = Guid.NewGuid() };

        _repositoryMock
            .Setup(r => r.GetById(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscriptionPlan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Plan_Has_Active_Subscriptions()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new DomainSubscriptionPlan
        {
            Id = planId,
            Name = "Test Plan",
            IsActive = true
        };

        var activeSubscription = new SellerSubscription
        {
            Id = Guid.NewGuid(),
            SubscriptionPlanId = planId,
            IsActive = true
        };

        var command = new DeleteSubscriptionPlanCommand { Id = planId };

        _repositoryMock
            .Setup(r => r.GetById(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var mockQueryable = new List<SellerSubscription> { activeSubscription }.AsQueryable().BuildMock();
        _sellerSubscriptionRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(mockQueryable);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Throw_Exception_When_Plan_Has_Inactive_Subscriptions()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var existingPlan = new DomainSubscriptionPlan
        {
            Id = planId,
            Name = "Test Plan",
            IsActive = true
        };

        var inactiveSubscription = new SellerSubscription
        {
            Id = Guid.NewGuid(),
            SubscriptionPlanId = planId,
            IsActive = false
        };

        var command = new DeleteSubscriptionPlanCommand { Id = planId };

        _repositoryMock
            .Setup(r => r.GetById(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var mockQueryable = new List<SellerSubscription> { inactiveSubscription }.AsQueryable().BuildMock();
        _sellerSubscriptionRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(mockQueryable);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
