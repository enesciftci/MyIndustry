using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Seller.GetSellerProfileQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;

namespace MyIndustry.Tests.Unit.Seller;

public class GetSellerProfileQueryHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<Domain.Aggregate.Favorite>> _favoriteRepositoryMock;
    private readonly GetSellerProfileQueryHandler _handler;

    public GetSellerProfileQueryHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _favoriteRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Favorite>>();
        _handler = new GetSellerProfileQueryHandler(
            _sellerRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _favoriteRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Profile_When_Seller_Exists()
    {
        var userId = Guid.NewGuid();
        var seller = new Domain.Aggregate.Seller
        {
            Id = userId,
            Title = "Profil Satıcı",
            Description = "Açıklama",
            IsActive = true,
            SellerInfo = new SellerInfo { Email = "x@y.com", PhoneNumber = "555" },
            SellerSubscriptions = new List<DomainSellerSubscription>()
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller> { seller }.AsQueryable().BuildMock());
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());
        _favoriteRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Favorite>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerProfileQuery { UserId = userId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Seller.Should().NotBeNull();
        result.Seller!.Title.Should().Be("Profil Satıcı");
        result.Seller.TotalServices.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Seller_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetSellerProfileQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Satıcı bulunamadı");
    }
}
