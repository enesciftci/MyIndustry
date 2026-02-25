using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.GetServiceBySlugQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.Tests.Unit.Service;

public class GetServiceBySlugQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly GetServiceBySlugQueryHandler _handler;

    public GetServiceBySlugQueryHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _handler = new GetServiceBySlugQueryHandler(_serviceRepositoryMock.Object, _categoryRepositoryMock.Object, _sellerRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Service_When_Slug_Matches()
    {
        var id = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var service = new Domain.Aggregate.Service
        {
            Id = id,
            Slug = "forklift-satilik",
            Title = "Forklift",
            Description = "Açıklama",
            Price = 100000,
            SellerId = sellerId,
            CategoryId = categoryId,
            Condition = ProductCondition.New,
            ListingType = ListingType.ForSale
        };
        var seller = new DomainSeller { Id = sellerId, Title = "Satıcı" };
        var categories = new List<DomainCategory> { new() { Id = categoryId, Name = "Kategori", ParentId = null } };

        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service> { service }.AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.GetById(sellerId, It.IsAny<CancellationToken>())).ReturnsAsync(seller);
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(categories.AsQueryable().BuildMock());

        var query = new GetServiceBySlugQuery { Slug = "forklift-satilik" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Service.Should().NotBeNull();
        result.Service!.Title.Should().Be("Forklift");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Service_Not_Found()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());

        var query = new GetServiceBySlugQuery { Slug = "yok-slug" };

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Not found");
    }
}
