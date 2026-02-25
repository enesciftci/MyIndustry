using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerByIdQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;

namespace MyIndustry.Tests.Unit.Seller;

public class GetSellerByIdQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly GetSellerByIdQueryHandler _handler;

    public GetSellerByIdQueryHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _handler = new GetSellerByIdQueryHandler(_sellerRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Seller_When_Found()
    {
        var sellerId = Guid.NewGuid();
        var seller = new Domain.Aggregate.Seller
        {
            Id = sellerId,
            Title = "Satıcı",
            Description = "Açıklama",
            Sector = SellerSector.IronMongery,
            IsActive = true,
            SellerInfo = new SellerInfo { SellerId = sellerId, Email = "a@b.com", PhoneNumber = "555" },
            SellerSubscriptions = new List<DomainSellerSubscription>(),
            Services = new List<Domain.Aggregate.Service>()
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller> { seller }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerByIdQuery { SellerId = sellerId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Seller.Should().NotBeNull();
        result.Seller!.Title.Should().Be("Satıcı");
        result.Seller.Email.Should().Be("a@b.com");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Seller_Does_Not_Exist()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerByIdQuery { SellerId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Seller.Should().BeNull();
    }
}
