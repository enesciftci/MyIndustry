using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Admin.ApproveListingCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainService = MyIndustry.Domain.Aggregate.Service;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;

namespace MyIndustry.Tests.Unit.Admin;

public class ApproveListingCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ApproveListingCommandHandler _handler;

    public ApproveListingCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ApproveListingCommandHandler(
            _serviceRepositoryMock.Object,
            _sellerRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Service_Not_Found()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new ApproveListingCommand { ServiceId = Guid.NewGuid(), Approve = true }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("İlan bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Approve_Listing_When_Approve_True()
    {
        var serviceId = Guid.NewGuid();
        var service = new DomainService
        {
            Id = serviceId,
            IsApproved = false,
            IsActive = false,
            Seller = new Domain.Aggregate.Seller { Id = Guid.NewGuid() }
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ApproveListingCommand { ServiceId = serviceId, Approve = true }, CancellationToken.None);

        result.Success.Should().BeTrue();
        service.IsApproved.Should().BeTrue();
        service.IsActive.Should().BeTrue();
        _serviceRepositoryMock.Verify(r => r.Update(service), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Reject_Listing_When_Approve_False()
    {
        var serviceId = Guid.NewGuid();
        var service = new DomainService
        {
            Id = serviceId,
            IsApproved = true,
            IsActive = true,
            IsFeatured = false,
            Seller = new Domain.Aggregate.Seller { Id = Guid.NewGuid(), SellerSubscriptions = new List<DomainSellerSubscription>() }
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ApproveListingCommand
        {
            ServiceId = serviceId,
            Approve = false,
            RejectionReasonType = RejectionReasonType.Other,
            RejectionReasonDescription = "Test"
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        service.IsApproved.Should().BeFalse();
        service.IsActive.Should().BeFalse();
        service.RejectionReasonType.Should().Be(RejectionReasonType.Other);
        service.RejectionReasonDescription.Should().Be("Test");
    }
}
