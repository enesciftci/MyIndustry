using FluentAssertions;
using Moq;
using MyIndustry.ApplicationService.Handler.Category.GetMainCategoriesQuery;
using MyIndustry.Repository.Repository;
using MyIndustry.Tests.Helpers;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.Tests.Unit.Category;

public class GetMainCategoriesQueryHandlerTests
{
    private readonly Mock<IGenericRepository<DomainCategory>> _categoryRepositoryMock;
    private readonly GetMainCategoriesQueryHandler _handler;

    public GetMainCategoriesQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IGenericRepository<DomainCategory>>();
        _handler = new GetMainCategoriesQueryHandler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Main_Categories()
    {
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(new List<DomainCategory>().AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetMainCategoriesQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Categories.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Root_Categories_ParentId_Null()
    {
        var main1 = new DomainCategory { Id = Guid.NewGuid(), Name = "Makina", ParentId = null };
        var main2 = new DomainCategory { Id = Guid.NewGuid(), Name = "Ekipman", ParentId = null };
        var child = new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = "Alt Kategori",
            ParentId = main1.Id
        };
        var categories = new List<DomainCategory> { main1, main2, child };
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(categories.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetMainCategoriesQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Categories.Should().HaveCount(2);
        result.Categories!.Select(c => c.Name).Should().Contain("Makina").And.Contain("Ekipman");
    }

    [Fact]
    public async Task Handle_Should_Map_Id_And_Name_To_CategoryDto()
    {
        var id = Guid.NewGuid();
        var main = new DomainCategory { Id = id, Name = "İş Makineleri", ParentId = null };
        _categoryRepositoryMock
            .Setup(r => r.GetAllQuery())
            .Returns(new List<DomainCategory> { main }.AsQueryable().BuildMock());

        var result = await _handler.Handle(new GetMainCategoriesQuery(), CancellationToken.None);

        result.Categories!.Should().ContainSingle();
        result.Categories[0].Id.Should().Be(id);
        result.Categories[0].Name.Should().Be("İş Makineleri");
    }
}
