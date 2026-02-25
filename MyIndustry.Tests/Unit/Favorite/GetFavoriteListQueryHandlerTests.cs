using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteListQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainFavorite = MyIndustry.Domain.Aggregate.Favorite;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Favorite;

public class GetFavoriteListQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainFavorite>> _favoriteRepositoryMock;
    private readonly GetFavoriteListQueryHandler _handler;

    public GetFavoriteListQueryHandlerTests()
    {
        _favoriteRepositoryMock = new Mock<IGenericRepository<DomainFavorite>>();
        _handler = new GetFavoriteListQueryHandler(_favoriteRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_User_Has_No_Favorites()
    {
        _favoriteRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainFavorite>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetFavoriteListQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Favorites.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Favorites_With_Service_When_User_Has_Favorites()
    {
        var userId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var service = new DomainService
        {
            Id = serviceId,
            Title = "İlan Başlık",
            Description = "Açıklama",
            Price = 1000m,
            ImageUrls = "url1,url2",
            SellerId = Guid.NewGuid(),
            EstimatedEndDay = 30,
            ViewCount = 5,
            City = "İstanbul",
            District = "Kadıköy",
            Neighborhood = "Moda"
        };
        var favorite = new DomainFavorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ServiceId = serviceId,
            Service = service
        };
        _favoriteRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainFavorite> { favorite }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetFavoriteListQuery { UserId = userId }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Favorites.Should().HaveCount(1);
        result.Favorites![0].IsFavorite.Should().BeTrue();
        result.Favorites[0].Service.Should().NotBeNull();
        result.Favorites[0].Service!.Title.Should().Be("İlan Başlık");
    }
}
