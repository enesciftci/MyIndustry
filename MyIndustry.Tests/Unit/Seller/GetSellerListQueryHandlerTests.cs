using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerListQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Provider;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCity = MyIndustry.Domain.Aggregate.City;

namespace MyIndustry.Tests.Unit.Seller;

public class GetSellerListQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<DomainCity>> _cityRepositoryMock;
    private readonly Mock<ISecurityProvider> _securityProviderMock;
    private readonly GetSellerListQueryHandler _handler;

    public GetSellerListQueryHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _cityRepositoryMock = new Mock<IGenericRepository<DomainCity>>();
        _securityProviderMock = new Mock<ISecurityProvider>();
        _handler = new GetSellerListQueryHandler(
            _sellerRepositoryMock.Object,
            _cityRepositoryMock.Object,
            _securityProviderMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Sellers()
    {
        _cityRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainCity>().AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerListQuery { Pager = new Pager(1, 10) }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Sellers.Should().NotBeNull().And.BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Active_Sellers_With_TotalCount()
    {
        var seller = new Domain.Aggregate.Seller
        {
            Id = Guid.NewGuid(),
            Title = "Satıcı A",
            Description = "",
            IsActive = true,
            Services = new List<Domain.Aggregate.Service>(),
            SellerInfo = null,
            Addresses = new List<Address>()
        };
        _cityRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainCity>().AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller> { seller }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerListQuery { Pager = new Pager(1, 10) }, CancellationToken.None);

        result.Sellers.Should().HaveCount(1);
        result.Sellers![0].Title.Should().Be("Satıcı A");
        result.TotalCount.Should().Be(1);
    }
}
