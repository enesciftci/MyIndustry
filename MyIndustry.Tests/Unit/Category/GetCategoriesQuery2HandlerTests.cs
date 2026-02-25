using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.Tests.Unit.Category;

public class GetCategoriesQuery2HandlerTests
{
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly GetCategoriesQuery2Handler _handler;

    public GetCategoriesQuery2HandlerTests()
    {
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _handler = new GetCategoriesQuery2Handler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_When_ParentId_Is_Null_Should_Return_Full_Tree()
    {
        var parent = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = "Kök",
            ParentId = null,
            IsActive = true
        };
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(new List<DomainCategory> { parent }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetCategoriesQuery2 { ParentId = null }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Categories.Should().HaveCount(1);
        result.Categories![0].Name.Should().Be("Kök");
    }

    [Fact]
    public async Task Handle_When_ParentId_Is_Set_Should_Return_Only_Children_Of_That_Parent()
    {
        var parentId = Guid.NewGuid();
        var child1 = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = "Alt 1",
            ParentId = parentId,
            IsActive = true
        };
        var child2 = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = "Alt 2",
            ParentId = parentId,
            IsActive = true
        };
        var categories = new List<DomainCategory> { child1, child2 };
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(categories.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetCategoriesQuery2 { ParentId = parentId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Categories.Should().HaveCount(2);
        result.Categories!.Select(c => c.Name).Should().Contain("Alt 1").And.Contain("Alt 2");
    }

    [Fact]
    public async Task Handle_When_ParentId_Has_No_Children_Should_Return_Empty_List()
    {
        var parentId = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(new List<DomainCategory>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetCategoriesQuery2 { ParentId = parentId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Categories.Should().NotBeNull().And.BeEmpty();
    }
}
