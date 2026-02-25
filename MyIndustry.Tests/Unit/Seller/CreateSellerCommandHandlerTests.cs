using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.Provider;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;

namespace MyIndustry.Tests.Unit.Seller;

public class CreateSellerCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Seller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSellerSubscription>> _sellerSubscriptionRepositoryMock;
    private readonly Mock<IGenericRepository<Domain.Aggregate.SubscriptionPlan>> _subscriptionPlanRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISecurityProvider> _securityProviderMock;
    private readonly CreateSellerCommandHandler _handler;

    public CreateSellerCommandHandlerTests()
    {
        _sellerRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Seller>>();
        _sellerSubscriptionRepositoryMock = new Mock<IGenericRepository<DomainSellerSubscription>>();
        _subscriptionPlanRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.SubscriptionPlan>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _securityProviderMock = new Mock<ISecurityProvider>();
        _handler = new CreateSellerCommandHandler(
            _sellerRepositoryMock.Object,
            _sellerSubscriptionRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _securityProviderMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_New_Seller_And_Free_Subscription_When_User_Not_Exists()
    {
        var userId = Guid.NewGuid();
        var freePlan = new Domain.Aggregate.SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            SubscriptionType = SubscriptionType.Free,
            IsActive = true,
            PostDurationInDays = 30,
            MonthlyPostLimit = 5,
            FeaturedPostLimit = 0
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Seller, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _subscriptionPlanRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.SubscriptionPlan> { freePlan }.AsQueryable().BuildMock());
        _securityProviderMock.Setup(s => s.EncryptAes256(It.IsAny<string>())).Returns("encrypted");
        _sellerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Aggregate.Seller>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _sellerSubscriptionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainSellerSubscription>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new CreateSellerCommand
        {
            UserId = userId,
            Title = "Satıcı",
            Description = "Açıklama",
            IdentityNumber = "123",
            Email = "a@b.com",
            PhoneNumber = "555",
            Sector = SellerSector.IronMongery
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _sellerRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Aggregate.Seller>(s => s.Id == userId && s.Title == "Satıcı"), It.IsAny<CancellationToken>()), Times.Once);
        _sellerSubscriptionRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainSellerSubscription>(s => s.SellerId == userId && s.IsActive), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Ok_When_Existing_Seller_Has_Active_Subscription()
    {
        var userId = Guid.NewGuid();
        var existingSeller = new Domain.Aggregate.Seller
        {
            Id = userId,
            SellerSubscriptions = new List<DomainSellerSubscription> { new() { IsActive = true } }
        };
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller> { existingSeller }.AsQueryable().BuildMock());

        var command = new CreateSellerCommand { UserId = userId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _sellerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Aggregate.Seller>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Identity_Already_Used_By_Another_Seller()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Seller, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _securityProviderMock.Setup(s => s.EncryptAes256(It.IsAny<string>())).Returns("enc");

        var command = new CreateSellerCommand { UserId = Guid.NewGuid(), IdentityNumber = "123" };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Aynı VKN/TCKN ile başka bir satıcı mevcut.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Free_Plan_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Seller>().AsQueryable().BuildMock());
        _sellerRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Seller, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _subscriptionPlanRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.SubscriptionPlan>().AsQueryable().BuildMock());
        _securityProviderMock.Setup(s => s.EncryptAes256(It.IsAny<string>())).Returns("enc");

        var command = new CreateSellerCommand { UserId = Guid.NewGuid() };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*abonelik planı bulunamadı*");
    }
}
