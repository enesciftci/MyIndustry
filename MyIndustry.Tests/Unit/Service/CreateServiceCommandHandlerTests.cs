using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;
using DomainSellerSubscription = MyIndustry.Domain.Aggregate.SellerSubscription;
using DomainSubscriptionPlan = MyIndustry.Domain.Aggregate.SubscriptionPlan;

namespace MyIndustry.Tests.Unit.Service;

public class CreateServiceCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Service>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGenericRepository<DomainSeller>> _sellerRepositoryMock;
    private readonly Mock<IGenericRepository<SubCategory>> _subCategoryRepositoryMock;
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly Mock<IGenericRepository<DomainSellerSubscription>> _sellerSubscriptionRepositoryMock;
    private readonly CreateServiceCommandHandler _handler;

    public CreateServiceCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Service>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sellerRepositoryMock = new Mock<IGenericRepository<DomainSeller>>();
        _subCategoryRepositoryMock = new Mock<IGenericRepository<SubCategory>>();
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _sellerSubscriptionRepositoryMock = new Mock<IGenericRepository<DomainSellerSubscription>>();
        _handler = new CreateServiceCommandHandler(
            _serviceRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _sellerRepositoryMock.Object,
            _subCategoryRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _sellerSubscriptionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Service_When_Valid()
    {
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
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
        var category = new DomainCategory { Id = categoryId, Name = "İş Makineleri", IsActive = true };

        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _categoryRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DomainCategory, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.GetById(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<Domain.Aggregate.Service>().AsQueryable().BuildMock());
        _serviceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Aggregate.Service>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new CreateServiceCommand
        {
            Title = "Forklift",
            Description = "Açıklama",
            Price = 100000,
            SellerId = sellerId,
            CategoryId = categoryId,
            EstimatedEndDay = 30,
            ImageUrls = "[]",
            Condition = ProductCondition.New,
            ListingType = ListingType.ForSale,
            IsFeatured = false
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Aggregate.Service>(s =>
            s.Title == "Forklift" &&
            s.SellerId == sellerId &&
            s.CategoryId == categoryId &&
            s.IsActive &&
            s.ExpiryDate != null
        ), It.IsAny<CancellationToken>()), Times.Once);
        activeSub.RemainingPostQuota.Should().Be(4);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Seller_Not_Found()
    {
        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller>().AsQueryable().BuildMock());

        var command = new CreateServiceCommand
        {
            Title = "X",
            Description = "Y",
            Price = 100,
            SellerId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid()
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Satıcı bulunamadı");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Category_Not_Found()
    {
        var sellerId = Guid.NewGuid();
        var plan = new DomainSubscriptionPlan { Id = Guid.NewGuid(), PostDurationInDays = 30, Name = "P", Description = "", SubscriptionType = SubscriptionType.Standard, MonthlyPrice = 0, MonthlyPostLimit = 5, FeaturedPostLimit = 0, IsActive = true };
        var activeSub = new DomainSellerSubscription { SellerId = sellerId, IsActive = true, RemainingPostQuota = 5, SubscriptionPlan = plan, SubscriptionPlanId = plan.Id };
        var seller = new DomainSeller { Id = sellerId, SellerSubscriptions = new List<DomainSellerSubscription> { activeSub } };

        _sellerRepositoryMock.Setup(r => r.GetAllQuery()).Returns(new List<DomainSeller> { seller }.AsQueryable().BuildMock());
        _categoryRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DomainCategory, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var command = new CreateServiceCommand
        {
            Title = "X",
            Description = "Y",
            Price = 100,
            SellerId = sellerId,
            CategoryId = Guid.NewGuid()
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Alt kategori bulunamadı.");
    }
}
