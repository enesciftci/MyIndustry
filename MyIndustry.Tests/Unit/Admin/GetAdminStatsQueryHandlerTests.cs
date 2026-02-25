using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Admin.GetAdminStatsQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Admin;

public class GetAdminStatsQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<DomainMessage>> _messageRepositoryMock;
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly GetAdminStatsQueryHandler _handler;

    public GetAdminStatsQueryHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _messageRepositoryMock = new Mock<IGenericRepository<DomainMessage>>();
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _handler = new GetAdminStatsQueryHandler(
            _sellerRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _messageRepositoryMock.Object,
            _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Zero_Stats_When_No_Data()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller>().AsQueryable().BuildMock());
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService>().AsQueryable().BuildMock());
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage>().AsQueryable().BuildMock());
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainCategory>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Stats.Should().NotBeNull();
        result.Stats!.TotalSellers.Should().Be(0);
        result.Stats.TotalListings.Should().Be(0);
        result.Stats.TotalMessages.Should().Be(0);
        result.Stats.TotalCategories.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Stats_When_Data_Exists()
    {
        var seller = new DomainSeller { Id = Guid.NewGuid(), Title = "S", CreatedDate = DateTime.UtcNow };
        var service = new DomainService
        {
            Id = Guid.NewGuid(),
            Title = "İlan",
            IsApproved = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainService> { service }.AsQueryable().BuildMock());
        _messageRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainMessage>().AsQueryable().BuildMock());
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainCategory>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.Stats!.TotalSellers.Should().Be(1);
        result.Stats.TotalListings.Should().Be(1);
        result.Stats.ApprovedListings.Should().Be(1);
        result.Stats.PendingListings.Should().Be(0);
        result.Stats.RecentActivities.Should().NotBeNull();
        result.Stats.UserRegistrationChart.Should().NotBeNull();
        result.Stats.ListingChart.Should().NotBeNull();
    }
}
