using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Admin.SuspendSellerCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.Tests.Unit.Admin;

public class SuspendSellerCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SuspendSellerCommandHandler _handler;

    public SuspendSellerCommandHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new SuspendSellerCommandHandler(_sellerRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Seller_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new SuspendSellerCommand { SellerId = Guid.NewGuid(), Suspend = true }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Satıcı bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Deactivate_Seller_When_Suspend_True()
    {
        var sellerId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId, IsActive = true };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new SuspendSellerCommand { SellerId = sellerId, Suspend = true }, CancellationToken.None);

        result.Success.Should().BeTrue();
        seller.IsActive.Should().BeFalse();
        _sellerRepositoryMock.Verify(r => r.Update(seller), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Activate_Seller_When_Suspend_False()
    {
        var sellerId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId, IsActive = false };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new SuspendSellerCommand { SellerId = sellerId, Suspend = false }, CancellationToken.None);

        result.Success.Should().BeTrue();
        seller.IsActive.Should().BeTrue();
    }
}
