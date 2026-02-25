using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Unit.Category;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Category>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Category>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Category_Successfully()
    {
        var emptyCategories = new List<Domain.Aggregate.Category>().AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(emptyCategories);
        _categoryRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Domain.Aggregate.Category>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new CreateCategoryCommand { Name = "Forklift", Description = "Açıklama" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Aggregate.Category>(c =>
            c.Name == "Forklift" &&
            c.Description == "Açıklama" &&
            c.IsActive &&
            !string.IsNullOrEmpty(c.Slug) &&
            c.MetaTitle == "Forklift | MyIndustry"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Name_Exists()
    {
        _categoryRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Aggregate.Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateCategoryCommand { Name = "Mevcut Kategori", Description = "" };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Kategori mevcut.");
    }
}
