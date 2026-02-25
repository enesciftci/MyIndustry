using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.Tests.Unit.Category;

public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _handler = new GetCategoriesQueryHandler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Tree_When_No_Categories()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(new List<DomainCategory>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Categories.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Tree_With_Root_And_Children()
    {
        var parentId = Guid.NewGuid();
        var parent = new DomainCategory
        {
            Id = parentId,
            Name = "Ana Kategori",
            Description = "Açıklama",
            ParentId = null,
            IsActive = true
        };
        var child = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = "Alt Kategori",
            Description = "",
            ParentId = parentId,
            IsActive = true
        };
        var categories = new List<DomainCategory> { parent, child };
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(categories.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Categories.Should().NotBeNull().And.HaveCount(1);
        result.Categories![0].Name.Should().Be("Ana Kategori");
        result.Categories[0].Children.Should().NotBeNull().And.HaveCount(1);
        result.Categories[0].Children![0].Name.Should().Be("Alt Kategori");
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Active_Categories()
    {
        var active = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = "Aktif",
            ParentId = null,
            IsActive = true
        };
        var inactive = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = "Pasif",
            ParentId = null,
            IsActive = false
        };
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(new List<DomainCategory> { active, inactive }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Categories.Should().HaveCount(1);
        result.Categories![0].Name.Should().Be("Aktif");
    }
}
