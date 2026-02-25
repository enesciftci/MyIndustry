using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.DeleteServiceByIdCommand;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;

namespace MyIndustry.Tests.Unit.Service;

public class DeleteServiceByIdCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteServiceByIdCommandHandler _handler;

    public DeleteServiceByIdCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteServiceByIdCommandHandler(_unitOfWorkMock.Object, _serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_And_Save()
    {
        _serviceRepositoryMock.Setup(r => r.Delete(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Service, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var command = new DeleteServiceByIdCommand { Id = id, SellerId = sellerId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _serviceRepositoryMock.Verify(r => r.Delete(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Service, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
