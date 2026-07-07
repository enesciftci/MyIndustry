using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.DeleteCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;
using MyIndustry.ApplicationService.Handler.Category.UpdateCategoryCommand;
using MyIndustry.Domain.Aggregate;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using MyIndustry.Repository.UnitOfWork;
using MyIndustry.Tests.Helpers;
using UnitOfWork = MyIndustry.Repository.UnitOfWork.UnitOfWork;

namespace MyIndustry.Tests.Integration.Category;

public class CategoryIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly IGenericRepository<DomainCategory> _categoryRepository;
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _categoryRepository = new GenericRepository<DomainCategory>(_context);
        _serviceRepository = new GenericRepository<Domain.Aggregate.Service>(_context);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task CreateCategory_Should_Save_With_Seo_Fields()
    {
        var handler = new CreateCategoryCommandHandler(_categoryRepository, _unitOfWork);
        var command = new CreateCategoryCommand
        {
            Name = "Metalworking",
            Description = "Metal processing equipment and tools"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        var saved = await _context.Categories.FirstOrDefaultAsync(c => c.Name == command.Name);
        saved.Should().NotBeNull();
        saved!.Slug.Should().NotBeNullOrEmpty();
        saved.MetaTitle.Should().Contain(command.Name);
        saved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSubCategory_Should_Link_To_Parent()
    {
        var parent = await TestDataBuilder.SeedCategoryAsync(_context, "Parent Category");
        var handler = new CreateSubCategoryCommandHandler(_categoryRepository, _unitOfWork);

        var result = await handler.Handle(
            new CreateSubCategoryCommand(new SubCategoryDto
            {
                CategoryId = parent.Id,
                Name = "Sub Category",
                Description = "Child category"
            }),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        var child = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Sub Category");
        child.Should().NotBeNull();
        child!.ParentId.Should().Be(parent.Id);
    }

    [Fact]
    public async Task GetCategories_Should_Return_Tree_Structure()
    {
        var parent = await TestDataBuilder.SeedCategoryAsync(_context, "Root Category");
        _context.Categories.Add(new Domain.Aggregate.Category
        {
            Id = Guid.NewGuid(),
            Name = "Child Category",
            Description = "Child",
            ParentId = parent.Id,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new GetCategoriesQueryHandler(_categoryRepository);
        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Categories.Should().HaveCount(1);
        result.Categories[0].Name.Should().Be("Root Category");
        result.Categories[0].Children.Should().HaveCount(1);
        result.Categories[0].Children[0].Name.Should().Be("Child Category");
    }

    [Fact]
    public async Task UpdateCategory_Should_Persist_Changes()
    {
        var existing = await TestDataBuilder.SeedCategoryAsync(_context, "Old Name");
        var handler = new UpdateCategoryCommandHandler(_categoryRepository, _unitOfWork);

        var result = await handler.Handle(new UpdateCategoryCommand
        {
            Id = existing.Id,
            Name = "New Name",
            Description = "Updated description",
            IsActive = true
        }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var updated = await _context.Categories.FindAsync(existing.Id);
        updated!.Name.Should().Be("New Name");
        updated.Description.Should().Be("Updated description");
        updated.ModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteCategory_Should_Remove_Empty_Category()
    {
        var category = await TestDataBuilder.SeedCategoryAsync(_context, "To Delete");
        var handler = new DeleteCategoryCommandHandler(_categoryRepository, _serviceRepository, _unitOfWork);

        var result = await handler.Handle(new DeleteCategoryCommand { Id = category.Id }, CancellationToken.None);

        result.Success.Should().BeTrue();
        var deleted = await _context.Categories.FindAsync(category.Id);
        deleted.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
