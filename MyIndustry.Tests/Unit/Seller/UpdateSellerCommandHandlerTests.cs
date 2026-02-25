using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.Provider;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;

namespace MyIndustry.Tests.Unit.Seller;

public class UpdateSellerCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISecurityProvider> _securityProviderMock;
    private readonly UpdateSellerCommandHandler _handler;

    public UpdateSellerCommandHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _securityProviderMock = new Mock<ISecurityProvider>();
        _handler = new UpdateSellerCommandHandler(
            _sellerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _securityProviderMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Seller_When_Found()
    {
        var id = Guid.NewGuid();
        var seller = new Domain.Aggregate.Seller
        {
            Id = id,
            Title = "Eski",
            Description = "Eski",
            Sector = SellerSector.IronMongery,
            SellerInfo = new SellerInfo { SellerId = id, Email = "old@x.com", PhoneNumber = "111" }
        };
        _sellerRepositoryMock.Setup(r => r.GetSingleOrDefault(id, It.IsAny<CancellationToken>())).ReturnsAsync(seller);
        _securityProviderMock.Setup(s => s.EncryptAes256(It.IsAny<string>())).Returns("encrypted");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new UpdateSellerCommand
        {
            Id = id,
            Title = "Yeni Başlık",
            Description = "Yeni açıklama",
            Email = "new@x.com",
            PhoneNumber = "222",
            IdentityNumber = "123",
            Sector = SellerSector.Dumper
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        seller.Title.Should().Be("Yeni Başlık");
        seller.SellerInfo.Email.Should().Be("new@x.com");
        _sellerRepositoryMock.Verify(r => r.Update(seller), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Seller_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetSingleOrDefault(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregate.Seller?)null);

        var command = new UpdateSellerCommand { Id = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Satıcı bulunamadı.");
    }
}
