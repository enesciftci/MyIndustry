using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Category.DeleteCategoryCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.Tests.Unit.Category;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly Mock<IGenericRepository<DomainService>> _serviceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _serviceRepositoryMock = new Mock<IGenericRepository<DomainService>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        var emptyList = new List<DomainCategory>().AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(emptyList);

        var command = new DeleteCategoryCommand { Id = Guid.NewGuid() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Category_Has_Listings()
    {
        var id = Guid.NewGuid();
        var category = new DomainCategory
        {
            Id = id,
            Name = "Test",
            Description = "",
            IsActive = true,
            Children = new List<DomainCategory>()
        };
        var categoryList = new List<DomainCategory> { category }.AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(categoryList);

        var servicesWithCategory = new List<DomainService>
        {
            new() { Id = Guid.NewGuid(), CategoryId = id, Title = "X", Description = "", Price = 0, SellerId = Guid.NewGuid() }
        };
        var serviceList = servicesWithCategory.AsQueryable().BuildMock();
        _serviceRepositoryMock.Setup(r => r.GetAllQuery()).Returns(serviceList);

        var command = new DeleteCategoryCommand { Id = id };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
