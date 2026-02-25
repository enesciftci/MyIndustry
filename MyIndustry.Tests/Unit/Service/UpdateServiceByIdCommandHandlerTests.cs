using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.Tests.Unit.Service;

public class UpdateServiceByIdCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateServiceByIdCommandHandler _handler;

    public UpdateServiceByIdCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateServiceByIdCommandHandler(
            _serviceRepositoryMock.Object,
            _sellerRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Service_When_Found()
    {
        var serviceId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = serviceId,
            SellerId = sellerId,
            Title = "Eski Başlık",
            Description = "Eski",
            Price = 1000,
            Slug = "eski-baslik",
            IsActive = true,
            CategoryId = categoryId,
            Condition = ProductCondition.New,
            ListingType = ListingType.ForSale,
            Seller = null
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service> { service }.AsQueryable().BuildMock());
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainCategory> { new DomainCategory { Id = categoryId, IsActive = true } }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var dto = new ServiceDto
        {
            Id = serviceId,
            SellerId = sellerId,
            Title = "Yeni Başlık",
            Description = "Yeni açıklama",
            Price = 2000,
            CategoryId = categoryId,
            EstimatedEndDay = 15,
            Condition = 0,
            ListingType = 0,
            IsFeatured = false
        };
        var command = new UpdateServiceByIdCommand { ServiceDto = dto };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        service.Title.Should().Be("Yeni Başlık");
        service.Description.Should().Be("Yeni açıklama");
        service.Price.Should().Be(2000);
        _serviceRepositoryMock.Verify(r => r.Update(service), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Service_Not_Found()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());

        var command = new UpdateServiceByIdCommand
        {
            ServiceDto = new ServiceDto
            {
                Id = Guid.NewGuid(),
                SellerId = Guid.NewGuid(),
                Title = "X",
                Description = "",
                Price = 0
            }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Servis bulunamadı.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Service_Is_Inactive()
    {
        var serviceId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = serviceId,
            SellerId = sellerId,
            IsActive = false,
            Title = "X",
            Description = "",
            Price = 0,
            CategoryId = Guid.NewGuid()
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service> { service }.AsQueryable().BuildMock());

        var command = new UpdateServiceByIdCommand
        {
            ServiceDto = new ServiceDto { Id = serviceId, SellerId = sellerId, Title = "X", Description = "", Price = 0 }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Servis bulunamadı.");
    }
}
