using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using MyIndustry.ApplicationService.Handler;

namespace MyIndustry.Tests.Unit.Service;

public class GetServicesBySearchTermQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _servicesRepositoryMock;
    private readonly GetServicesBySearchTermQueryHandler _handler;

    public GetServicesBySearchTermQueryHandlerTests()
    {
        _servicesRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _handler = new GetServicesBySearchTermQueryHandler(_servicesRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_Query_Is_Empty()
    {
        var query = new GetServicesBySearchTermQuery { Query = "", Pager = new Pager(1, 10) };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Services.Should().NotBeNull().And.BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_Query_Is_WhiteSpace()
    {
        var query = new GetServicesBySearchTermQuery { Query = "   ", Pager = new Pager(1, 10) };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Services.Should().NotBeNull().And.BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Services_When_Title_Matches_SearchTerm()
    {
        var now = DateTime.UtcNow.AddMinutes(10);
        var services = new List<Domain.Aggregate.Service>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "kompresör satılık",
                Description = "Açıklama",
                Price = 5000,
                SellerId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                IsApproved = true,
                ExpiryDate = now,
                Condition = ProductCondition.New,
                ListingType = ListingType.ForSale
            }
        };
        _servicesRepositoryMock.Setup(r => r.GetAllQuery()).Returns(services.AsQueryable().BuildMock());

        var query = new GetServicesBySearchTermQuery { Query = "kompresör", Pager = new Pager(1, 10) };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(1);
        result.Services.Should().NotBeNull().And.HaveCount(1);
        result.Services![0].Title.Should().Be("kompresör satılık");
    }
}
