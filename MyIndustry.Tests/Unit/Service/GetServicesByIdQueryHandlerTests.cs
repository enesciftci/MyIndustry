using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.Tests.Unit.Service;

public class GetServicesByIdQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly GetServicesByIdQueryHandler _handler;

    public GetServicesByIdQueryHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _handler = new GetServicesByIdQueryHandler(_serviceRepositoryMock.Object, _categoryRepositoryMock.Object, _sellerRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Service_When_Found()
    {
        var id = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = id,
            Title = "Forklift",
            Description = "Açıklama",
            Price = 100000,
            SellerId = sellerId,
            CategoryId = categoryId,
            Condition = ProductCondition.New,
            ListingType = ListingType.ForSale
        };
        var seller = new DomainSeller { Id = sellerId, Title = "Satıcı A" };
        var categories = new List<DomainCategory> { new() { Id = categoryId, Name = "İş Makineleri", ParentId = null } };

        _serviceRepositoryMock.Setup(r => r.GetById(id, It.IsAny<CancellationToken>())).ReturnsAsync(service);
        _sellerRepositoryMock.Setup(r => r.GetById(sellerId, It.IsAny<CancellationToken>())).ReturnsAsync(seller);
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(categories.AsQueryable().BuildMock());

        var query = new GetServicesByIdQuery { Id = id };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Service.Should().NotBeNull();
        result.Service!.Id.Should().Be(id);
        result.Service.Title.Should().Be("Forklift");
        result.Service.Seller.Should().NotBeNull();
        result.Service.Seller!.Title.Should().Be("Satıcı A");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Service_Not_Found()
    {
        _serviceRepositoryMock.Setup(r => r.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Aggregate.Service?)null);

        var query = new GetServicesByIdQuery { Id = Guid.NewGuid() };

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Not found");
    }
}
