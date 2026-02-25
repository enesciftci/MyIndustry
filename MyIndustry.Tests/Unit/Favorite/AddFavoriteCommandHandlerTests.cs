using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Favorite.AddFavoriteCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;

namespace MyIndustry.Tests.Unit.Favorite;

public class AddFavoriteCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Favorite>> _favoriteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddFavoriteCommandHandler _handler;

    public AddFavoriteCommandHandlerTests()
    {
        _favoriteRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Favorite>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new AddFavoriteCommandHandler(_unitOfWorkMock.Object, _favoriteRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Add_Favorite_When_Not_Exists()
    {
        _favoriteRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Favorite, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _favoriteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Aggregate.Favorite>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var userId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var command = new AddFavoriteCommand { UserId = userId, ServiceId = serviceId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _favoriteRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Aggregate.Favorite>(f =>
            f.UserId == userId && f.ServiceId == serviceId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Ok_Without_Add_When_Already_Exists()
    {
        _favoriteRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Favorite, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new AddFavoriteCommand { UserId = Guid.NewGuid(), ServiceId = Guid.NewGuid() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _favoriteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Aggregate.Favorite>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
