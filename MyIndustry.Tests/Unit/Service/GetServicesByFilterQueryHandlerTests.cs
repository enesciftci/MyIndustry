using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.Tests.Unit.Service;

public class GetServicesByFilterQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly GetServicesByFilterQueryHandler _handler;

    public GetServicesByFilterQueryHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _handler = new GetServicesByFilterQueryHandler(_serviceRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Matches()
    {
        var now = DateTime.UtcNow.AddMinutes(10);
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainCategory>().AsQueryable().BuildMock());

        var query = new GetServicesByFilterQuery { Index = 1, Size = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(0);
        result.Services.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Services_With_Seller_When_Data_Exists()
    {
        var now = DateTime.UtcNow.AddMinutes(10);
        var sellerId = Guid.NewGuid();
        var seller = new DomainSeller { Id = sellerId, Title = "Satıcı" };
        var services = new List<Domain.Aggregate.Service>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Forklift",
                Description = "Açıklama",
                Price = 100000,
                SellerId = sellerId,
                Seller = seller,
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                IsApproved = true,
                ExpiryDate = now,
                Condition = ProductCondition.New,
                ListingType = ListingType.ForSale
            }
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(services.AsQueryable().BuildMock());
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainCategory>().AsQueryable().BuildMock());

        var query = new GetServicesByFilterQuery { Index = 1, Size = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(1);
        result.Services.Should().NotBeNull().And.HaveCount(1);
        result.Services![0].Title.Should().Be("Forklift");
        result.Services[0].Seller.Should().NotBeNull();
        result.Services[0].Seller!.Title.Should().Be("Satıcı");
    }
}
