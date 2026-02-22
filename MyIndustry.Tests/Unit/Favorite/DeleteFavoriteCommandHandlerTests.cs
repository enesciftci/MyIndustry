using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Favorite.DeleteFavoriteCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Unit.Favorite;

public class DeleteFavoriteCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Favorite>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteFavoriteCommandHandler _handler;

    public DeleteFavoriteCommandHandlerTests()
    {
        _repositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Favorite>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteFavoriteCommandHandler(_unitOfWorkMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Favorite_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var favoriteId = Guid.NewGuid();
        var favorite = new Domain.Aggregate.Favorite
        {
            Id = favoriteId,
            UserId = userId,
            ServiceId = Guid.NewGuid()
        };

        var command = new DeleteFavoriteCommand
        {
            UserId = userId,
            FavoriteId = favoriteId
        };

        var mockQueryable = new List<Domain.Aggregate.Favorite> { favorite }.AsQueryable().BuildMock();
        _repositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(mockQueryable);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(favorite), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Favorite_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var favoriteId = Guid.NewGuid();

        var command = new DeleteFavoriteCommand
        {
            UserId = userId,
            FavoriteId = favoriteId,
            ServiceId = Guid.Empty
        };

        var mockQueryable = new List<Domain.Aggregate.Favorite>().AsQueryable().BuildMock();
        _repositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(mockQueryable);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Aggregate.Favorite>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Delete_Favorite_By_ServiceId_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var favorite = new Domain.Aggregate.Favorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ServiceId = serviceId
        };

        var command = new DeleteFavoriteCommand
        {
            UserId = userId,
            FavoriteId = Guid.Empty, // Not using FavoriteId
            ServiceId = serviceId
        };

        var mockQueryable = new List<Domain.Aggregate.Favorite> { favorite }.AsQueryable().BuildMock();
        _repositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(mockQueryable);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(favorite), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
