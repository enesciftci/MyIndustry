using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.IncreaseServiceViewCountCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;

namespace MyIndustry.Tests.Unit.Service;

public class IncreaseServiceViewCountCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IncreaseServiceViewCountCommandHandler _handler;

    public IncreaseServiceViewCountCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new IncreaseServiceViewCountCommandHandler(_unitOfWorkMock.Object, _serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Increment_ViewCount_And_Update()
    {
        var serviceId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = serviceId,
            Title = "Test",
            Description = "",
            Price = 0,
            ViewCount = 10,
            SellerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid()
        };
        _serviceRepositoryMock.Setup(r => r.GetById(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new IncreaseServiceViewCountCommand { ServiceId = serviceId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        service.ViewCount.Should().Be(11);
        _serviceRepositoryMock.Verify(r => r.Update(service), Times.Once);
    }
}
