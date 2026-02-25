using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.DisableServiceByIdCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.Tests.Unit.Service;

public class DisableServiceByIdCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<SubCategory>> _subCategoryRepositoryMock;
    private readonly DisableServiceByIdCommandHandler _handler;

    public DisableServiceByIdCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _subCategoryRepositoryMock = new Mock<IGenericRepository<SubCategory>>();
        _handler = new DisableServiceByIdCommandHandler(
            _serviceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _sellerRepositoryMock.Object,
            _subCategoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Disable_Service_Successfully()
    {
        var serviceId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = serviceId,
            SellerId = sellerId,
            IsActive = true,
            Title = "Test",
            Description = "",
            Price = 0,
            CategoryId = Guid.NewGuid()
        };

        _serviceRepositoryMock.Setup(r => r.GetById(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new DisableServiceByIdCommand { ServiceId = serviceId, SellerId = sellerId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        service.IsActive.Should().BeFalse();
        _serviceRepositoryMock.Verify(r => r.Update(service), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Service_Not_Found()
    {
        _serviceRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregate.Service?)null);

        var command = new DisableServiceByIdCommand { ServiceId = Guid.NewGuid(), SellerId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Servis bulunamadı.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_SellerId_Mismatch()
    {
        var serviceId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = serviceId,
            SellerId = Guid.NewGuid(),
            IsActive = true,
            Title = "Test",
            Description = "",
            Price = 0,
            CategoryId = Guid.NewGuid()
        };
        _serviceRepositoryMock.Setup(r => r.GetById(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var command = new DisableServiceByIdCommand { ServiceId = serviceId, SellerId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Servis bulunamadı.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Service_Already_Inactive()
    {
        var serviceId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = serviceId,
            SellerId = sellerId,
            IsActive = false,
            Title = "Test",
            Description = "",
            Price = 0,
            CategoryId = Guid.NewGuid()
        };
        _serviceRepositoryMock.Setup(r => r.GetById(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(service);

        var command = new DisableServiceByIdCommand { ServiceId = serviceId, SellerId = sellerId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("İlan zaten pasif.");
    }
}
