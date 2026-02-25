using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using MyIndustry.ApplicationService.Handler;

namespace MyIndustry.Tests.Unit.Service;

public class GetServicesByRandomlyQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly GetServicesByRandomlyQueryHandler _handler;

    public GetServicesByRandomlyQueryHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _handler = new GetServicesByRandomlyQueryHandler(_serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Services()
    {
        var now = DateTime.UtcNow.AddMinutes(5);
        var services = new List<Domain.Aggregate.Service>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "A",
                Description = "Desc",
                Price = 100,
                SellerId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                IsApproved = true,
                ExpiryDate = now,
                Condition = ProductCondition.New,
                ListingType = ListingType.ForSale
            }
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(services.AsQueryable().BuildMock());

        var query = new GetServicesByRandomlyQuery { Pager = new Pager(1, 10) };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Services.Should().NotBeNull();
        result.Services.Should().HaveCount(1);
        result.Services![0].Title.Should().Be("A");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Services()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());

        var query = new GetServicesByRandomlyQuery { Pager = new Pager(1, 10) };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Services.Should().NotBeNull().And.BeEmpty();
    }
}
