using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Admin.SuspendListingCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Admin;

public class SuspendListingCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SuspendListingCommandHandler _handler;

    public SuspendListingCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new SuspendListingCommandHandler(_serviceRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Service_Not_Found()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new SuspendListingCommand { ServiceId = Guid.NewGuid(), Suspend = true }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("İlan bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Deactivate_When_Suspend_True()
    {
        var serviceId = Guid.NewGuid();
        var service = new DomainService { Id = serviceId, IsActive = true };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new SuspendListingCommand
        {
            ServiceId = serviceId,
            Suspend = true,
            SuspensionReasonType = SuspensionReasonType.PolicyViolation,
            SuspensionReasonDescription = "Test"
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        service.IsActive.Should().BeFalse();
        service.SuspensionReasonType.Should().Be(SuspensionReasonType.PolicyViolation);
        service.SuspensionReasonDescription.Should().Be("Test");
    }

    [Fact]
    public async Task Handle_Should_Activate_When_Suspend_False()
    {
        var serviceId = Guid.NewGuid();
        var service = new DomainService
        {
            Id = serviceId,
            IsActive = false,
            SuspensionReasonType = SuspensionReasonType.PolicyViolation,
            SuspensionReasonDescription = "Old"
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new SuspendListingCommand { ServiceId = serviceId, Suspend = false }, CancellationToken.None);

        result.Success.Should().BeTrue();
        service.IsActive.Should().BeTrue();
        service.SuspensionReasonType.Should().BeNull();
        service.SuspensionReasonDescription.Should().BeNull();
    }
}
