using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminListingsQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Admin;

public class GetAdminListingsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly GetAdminListingsQueryHandler _handler;

    public GetAdminListingsQueryHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _handler = new GetAdminListingsQueryHandler(_serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Listings()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAdminListingsQuery { Index = 1, Size = 20 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(0);
        result.Listings.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Listings_With_TotalCount()
    {
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var seller = new Domain.Aggregate.Seller { Id = sellerId, Title = "Satıcı", SellerInfo = null };
        var category = new DomainCategory { Id = categoryId, Name = "Kategori" };
        var service = new DomainService
        {
            Id = Guid.NewGuid(),
            Title = "İlan",
            Description = "Açıklama",
            Price = 1000,
            SellerId = sellerId,
            Seller = seller,
            CategoryId = categoryId,
            Category = category,
            IsApproved = true,
            IsActive = true,
            ImageUrls = "[]"
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAdminListingsQuery { Index = 1, Size = 20 }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Listings.Should().HaveCount(1);
        result.Listings![0].Title.Should().Be("İlan");
        result.Listings[0].SellerName.Should().Be("Satıcı");
        result.Listings[0].Status.Should().Be("approved");
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Pending_Status()
    {
        var service = new DomainService
        {
            Id = Guid.NewGuid(),
            Title = "Bekleyen",
            IsApproved = false,
            IsActive = true,
            Seller = new Domain.Aggregate.Seller { Title = "S" },
            Category = new DomainCategory { Name = "K" }
        };
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAdminListingsQuery { Index = 1, Size = 20, Status = "pending" }, CancellationToken.None);

        result.Listings.Should().ContainSingle();
        result.Listings![0].Status.Should().Be("pending");
    }
}
