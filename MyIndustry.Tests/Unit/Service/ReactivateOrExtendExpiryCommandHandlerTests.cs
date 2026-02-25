using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.ReactivateOrExtendExpiryCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.Service;

public class ReactivateOrExtendExpiryCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSellerSubscription>> _sellerSubscriptionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ReactivateOrExtendExpiryCommandHandler _handler;

    public ReactivateOrExtendExpiryCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _sellerSubscriptionRepositoryMock = new Mock<IGenericRepository<DomainSellerSubscription>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new ReactivateOrExtendExpiryCommandHandler(
            _serviceRepositoryMock.Object,
            _sellerRepositoryMock.Object,
            _sellerSubscriptionRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Reactivate_And_Extend_Expiry_When_Service_Was_Inactive()
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
        var plan = new DomainSubscriptionPlan
        {
            Id = Guid.NewGuid(),
            PostDurationInDays = 30,
            Name = "Standard",
            Description = "",
            SubscriptionType = SubscriptionType.Standard,
            MonthlyPrice = 100,
            MonthlyPostLimit = 10,
            FeaturedPostLimit = 2,
            IsActive = true
        };
        var activeSub = new DomainSellerSubscription
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            IsActive = true,
            RemainingPostQuota = 5,
            SubscriptionPlan = plan,
            SubscriptionPlanId = plan.Id
        };
        var seller = new DomainSeller
        {
            Id = sellerId,
            Title = "Satıcı",
            SellerSubscriptions = new List<DomainSellerSubscription> { activeSub }
        };

        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service> { service }.AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new ReactivateOrExtendExpiryCommand { ServiceId = serviceId, SellerId = sellerId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        service.IsActive.Should().BeTrue();
        service.ExpiryDate.Should().NotBeNull();
        activeSub.RemainingPostQuota.Should().Be(4);
        _serviceRepositoryMock.Verify(r => r.Update(service), Times.Once);
        _sellerSubscriptionRepositoryMock.Verify(r => r.Update(activeSub), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Service_Not_Found()
    {
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());

        var command = new ReactivateOrExtendExpiryCommand { ServiceId = Guid.NewGuid(), SellerId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("İlan bulunamadı.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Seller_Not_Found()
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
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service> { service }.AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller>().AsQueryable().BuildMock());

        var command = new ReactivateOrExtendExpiryCommand { ServiceId = serviceId, SellerId = sellerId };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Satıcı bulunamadı.");
    }
}
