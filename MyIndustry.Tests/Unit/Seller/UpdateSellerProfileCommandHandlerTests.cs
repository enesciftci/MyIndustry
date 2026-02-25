using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Seller.UpdateSellerProfileCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Unit.Seller;

public class UpdateSellerProfileCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<SellerInfo>> _sellerInfoRepositoryMock;
    private readonly UpdateSellerProfileCommandHandler _handler;

    public UpdateSellerProfileCommandHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _sellerInfoRepositoryMock = new Mock<IGenericRepository<SellerInfo>>();
        _handler = new UpdateSellerProfileCommandHandler(
            _sellerRepositoryMock.Object,
            _sellerInfoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Profile_When_Seller_Exists()
    {
        var userId = Guid.NewGuid();
        var seller = new Domain.Aggregate.Seller
        {
            Id = userId,
            Title = "Eski",
            Description = "Eski",
            Sector = SellerSector.IronMongery,
            SellerInfo = new SellerInfo { SellerId = userId, LogoUrl = "old.png", WebSiteUrl = "old.com" }
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller> { seller }.AsQueryable().BuildMock());

        var command = new UpdateSellerProfileCommand
        {
            UserId = userId,
            Title = "Yeni Başlık",
            LogoUrl = "new.png",
            WebSiteUrl = "new.com"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        seller.Title.Should().Be("Yeni Başlık");
        seller.SellerInfo.LogoUrl.Should().Be("new.png");
        seller.SellerInfo.WebSiteUrl.Should().Be("new.com");
        _sellerInfoRepositoryMock.Verify(r => r.Update(seller.SellerInfo), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Seller_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new UpdateSellerProfileCommand { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Satıcı bulunamadı");
    }
}
