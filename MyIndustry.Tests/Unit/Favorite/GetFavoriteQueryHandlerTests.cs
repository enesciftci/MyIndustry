using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainFavorite = MyIndustry.Domain.Aggregate.Favorite;

namespace MyIndustry.Tests.Unit.Favorite;

public class GetFavoriteQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainFavorite>> _favoriteRepositoryMock;
    private readonly GetFavoriteQueryHandler _handler;

    public GetFavoriteQueryHandlerTests()
    {
        _favoriteRepositoryMock = new Mock<IGenericRepository<DomainFavorite>>();
        _handler = new GetFavoriteQueryHandler(_favoriteRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Ok_With_No_FavoriteDto_When_Not_Favorited()
    {
        _favoriteRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainFavorite>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetFavoriteQuery { UserId = Guid.NewGuid(), ServiceId = Guid.NewGuid() }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FavoriteDto.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_FavoriteDto_When_Favorite_Exists()
    {
        var userId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var favoriteId = Guid.NewGuid();
        var favorite = new DomainFavorite { Id = favoriteId, UserId = userId, ServiceId = serviceId };
        _favoriteRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainFavorite> { favorite }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetFavoriteQuery { UserId = userId, ServiceId = serviceId }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.FavoriteDto.Should().NotBeNull();
        result.FavoriteDto!.Id.Should().Be(favoriteId);
        result.FavoriteDto.IsFavorite.Should().BeTrue();
    }
}
