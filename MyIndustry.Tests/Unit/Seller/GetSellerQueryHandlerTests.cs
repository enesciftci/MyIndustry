using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Provider;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using SellerSector = MyIndustry.Domain.ValueObjects.SellerSector;

namespace MyIndustry.Tests.Unit.Seller;

public class GetSellerQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly Mock<ISecurityProvider> _securityProviderMock;
    private readonly GetSellerQueryHandler _handler;

    public GetSellerQueryHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _securityProviderMock = new Mock<ISecurityProvider>();
        _handler = new GetSellerQueryHandler(_sellerRepositoryMock.Object, _securityProviderMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Seller_When_Found()
    {
        var id = Guid.NewGuid();
        var seller = new Domain.Aggregate.Seller
        {
            Id = id,
            Title = "Satıcı",
            Description = "Açıklama",
            Sector = SellerSector.IronMongery
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller> { seller }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerQuery { Id = id }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Seller.Should().NotBeNull();
        result.Seller!.Id.Should().Be(id);
        result.Seller.Title.Should().Be("Satıcı");
    }

    [Fact]
    public async Task Handle_Should_Return_Null_Seller_When_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        result.Seller.Should().BeNull();
    }
}
