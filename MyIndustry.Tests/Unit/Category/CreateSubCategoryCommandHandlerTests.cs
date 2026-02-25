using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ExceptionHandling;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Unit.Category;

public class CreateSubCategoryCommandHandlerTests
{
    private readonly Mock<IGenericRepository<Domain.Aggregate.Category>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateSubCategoryCommandHandler _handler;

    public CreateSubCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IGenericRepository<Domain.Aggregate.Category>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateSubCategoryCommandHandler(_categoryRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_SubCategory_Successfully()
    {
        var parentId = Guid.NewGuid();
        var parentCategory = new Domain.Aggregate.Category
        {
            Id = parentId,
            Name = "Dorse",
            Slug = "dorse",
            IsActive = true,
            Description = ""
        };
        var categories = new List<Domain.Aggregate.Category> { parentCategory };
        var mockQueryable = categories.AsQueryable().BuildMock();

        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(mockQueryable);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Domain.Aggregate.Category>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new CreateSubCategoryCommand(new SubCategoryDto
        {
            CategoryId = parentId,
            Name = "Kılçık",
            Description = "Alt kategori açıklaması"
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Aggregate.Category>(c =>
            c.ParentId == parentId &&
            c.Name == "Kılçık" &&
            c.Description == "Alt kategori açıklaması" &&
            c.IsActive &&
            !string.IsNullOrEmpty(c.Slug) &&
            c.MetaTitle == "Kılçık | MyIndustry"
        ), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_SubCategory_Is_Null()
    {
        var command = new CreateSubCategoryCommand(null!);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Üst kategori seçilmedi.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_CategoryId_Is_Empty()
    {
        var command = new CreateSubCategoryCommand(new SubCategoryDto
        {
            CategoryId = Guid.Empty,
            Name = "Alt",
            Description = ""
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Üst kategori seçilmedi.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Parent_Not_Found()
    {
        var parentId = Guid.NewGuid();
        var categories = new List<Domain.Aggregate.Category>();
        var mockQueryable = categories.AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(mockQueryable);

        var command = new CreateSubCategoryCommand(new SubCategoryDto
        {
            CategoryId = parentId,
            Name = "Alt",
            Description = ""
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Üst kategori bulunamadı.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Name_Is_Empty()
    {
        var parentId = Guid.NewGuid();
        var parentCategory = new Domain.Aggregate.Category
        {
            Id = parentId,
            Name = "Parent",
            Slug = "parent",
            IsActive = true,
            Description = ""
        };
        var mockQueryable = new List<Domain.Aggregate.Category> { parentCategory }.AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(mockQueryable);

        var command = new CreateSubCategoryCommand(new SubCategoryDto
        {
            CategoryId = parentId,
            Name = "",
            Description = ""
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Alt kategori adı gerekli.");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Name_Is_WhiteSpace()
    {
        var parentId = Guid.NewGuid();
        var parentCategory = new Domain.Aggregate.Category
        {
            Id = parentId,
            Name = "Parent",
            Slug = "parent",
            IsActive = true,
            Description = ""
        };
        var mockQueryable = new List<Domain.Aggregate.Category> { parentCategory }.AsQueryable().BuildMock();
        _categoryRepositoryMock.Setup(r => r.GetAllQuery()).Returns(mockQueryable);

        var command = new CreateSubCategoryCommand(new SubCategoryDto
        {
            CategoryId = parentId,
            Name = "   ",
            Description = ""
        });

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Alt kategori adı gerekli.");
    }
}
