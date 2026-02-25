using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using MyIndustry.ApplicationService.Handler;

namespace MyIndustry.Tests.Unit.Service;

public class GetServicesBySellerIdQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _servicesRepositoryMock;
    private readonly Mock<IGenericRepository<Domain.Aggregate.Favorite>> _favoritesRepositoryMock;
    private readonly GetServicesBySellerIdQueryHandler _handler;

    public GetServicesBySellerIdQueryHandlerTests()
    {
        _servicesRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _favoritesRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Favorite>>();
        _handler = new GetServicesBySellerIdQueryHandler(_servicesRepositoryMock.Object, _favoritesRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Seller_Services()
    {
        var sellerId = Guid.NewGuid();
        var services = new List<Domain.Aggregate.Service>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "İlan 1",
                Description = "Açıklama",
                Price = 1000,
                SellerId = sellerId,
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                IsApproved = true,
                Condition = ProductCondition.New,
                ListingType = ListingType.ForSale
            }
        };
        _servicesRepositoryMock.Setup(r => r.GetAllQuery()).Returns(services.AsQueryable().BuildMock());
        _favoritesRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Favorite>().AsQueryable().BuildMock());

        var query = new GetServicesBySellerIdQuery { SellerId = sellerId, Pager = new Pager(1, 10) };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Services.Should().NotBeNull().And.HaveCount(1);
        result.Services![0].Title.Should().Be("İlan 1");
        result.Services[0].SellerId.Should().Be(sellerId);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_Seller_Has_No_Services()
    {
        var sellerId = Guid.NewGuid();
        _servicesRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());
        _favoritesRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Favorite>().AsQueryable().BuildMock());

        var query = new GetServicesBySellerIdQuery { SellerId = sellerId, Pager = new Pager(1, 10) };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Services.Should().NotBeNull().And.BeEmpty();
    }
}
