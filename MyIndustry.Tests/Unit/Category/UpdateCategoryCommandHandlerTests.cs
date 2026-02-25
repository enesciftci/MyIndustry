using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Category.UpdateCategoryCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Unit.Category;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Category>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Category>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateCategoryCommandHandler(_categoryRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Category_Successfully()
    {
        var id = Guid.NewGuid();
        var category = new Domain.Aggregate.Category
        {
            Id = id,
            Name = "Eski",
            Slug = "eski",
            Description = "",
            IsActive = true,
            MetaTitle = "",
            MetaDescription = "",
            MetaKeywords = ""
        };
        var list = new List<Domain.Aggregate.Category> { category };
        var mockQueryable = list.AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(mockQueryable);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new UpdateCategoryCommand { Id = id, Name = "Yeni Ad", Description = "Yeni açıklama", IsActive = true };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        category.Name.Should().Be("Yeni Ad");
        category.Description.Should().Be("Yeni açıklama");
        category.MetaTitle.Should().Be("Yeni Ad | MyIndustry");
        _categoryRepositoryMock.Verify(r => r.Update(category), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        var emptyList = new List<Domain.Aggregate.Category>().AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(emptyList);

        var command = new UpdateCategoryCommand { Id = Guid.NewGuid(), Name = "Yok", Description = "", IsActive = true };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }
}
