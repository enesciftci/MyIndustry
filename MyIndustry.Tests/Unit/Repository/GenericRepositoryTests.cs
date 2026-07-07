using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Repository.DbContext;
using MyIndustry.Repository.Repository;
using DomainCategory = MyIndustry.Domain.Aggregate.Category;

namespace MyIndustry.Tests.Unit.Repository;

public class GenericRepositoryTests : IDisposable
{
    private readonly MyIndustryDbContext _context;
    private readonly GenericRepository<DomainCategory> _repository;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MyIndustryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new MyIndustryDbContext(options);
        _repository = new GenericRepository<DomainCategory>(_context);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Entity()
    {
        var category = CreateCategory("Add Test");

        await _repository.AddAsync(category, CancellationToken.None);
        await _context.SaveChangesAsync();

        var stored = await _context.Categories.FindAsync(category.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Add Test");
    }

    [Fact]
    public async Task AddRange_Should_Add_Multiple_Entities()
    {
        var categories = new[]
        {
            CreateCategory("Range 1"),
            CreateCategory("Range 2")
        };

        await _repository.AddRange(categories, CancellationToken.None);
        await _context.SaveChangesAsync();

        (await _context.Categories.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Delete_By_Entity_Should_Remove_Entity()
    {
        var category = await SeedCategoryAsync("Delete Entity");

        _repository.Delete(category);
        await _context.SaveChangesAsync();

        (await _context.Categories.FindAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_By_Id_Should_Remove_Entity()
    {
        var category = await SeedCategoryAsync("Delete By Id");

        await _repository.Delete(category.Id, CancellationToken.None);
        await _context.SaveChangesAsync();

        (await _context.Categories.FindAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_By_Predicate_Should_Remove_Entity()
    {
        var category = await SeedCategoryAsync("Delete Predicate");

        await _repository.Delete(c => c.Name == "Delete Predicate", CancellationToken.None);
        await _context.SaveChangesAsync();

        (await _context.Categories.FindAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteRange_Should_Remove_Multiple_Entities()
    {
        var first = await SeedCategoryAsync("Delete Range 1");
        var second = await SeedCategoryAsync("Delete Range 2");

        _repository.DeleteRange(new List<DomainCategory> { first, second });
        await _context.SaveChangesAsync();

        (await _context.Categories.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetAll_Should_Return_All_Entities()
    {
        await SeedCategoryAsync("All 1");
        await SeedCategoryAsync("All 2");

        var result = await _repository.GetAll(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllQuery_Should_Return_AsNoTracking_Queryable()
    {
        await SeedCategoryAsync("Query Test");

        var query = _repository.GetAllQuery();
        var result = await query.ToListAsync();

        result.Should().HaveCount(1);
        _context.Entry(result[0]).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetAll_With_Predicate_Should_Filter_Entities()
    {
        await SeedCategoryAsync("Active", true);
        await SeedCategoryAsync("Inactive", false);

        var result = await _repository.GetAll(c => c.IsActive, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetById_Should_Return_Entity_By_Id()
    {
        var category = await SeedCategoryAsync("By Id");

        var result = await _repository.GetById(category.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetSingleOrDefault_Should_Return_Single_Entity()
    {
        var category = await SeedCategoryAsync("Single");

        var result = await _repository.GetSingleOrDefault(category.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Single");
    }

    [Fact]
    public async Task GetById_With_Predicate_Should_Return_Matching_Entity()
    {
        var category = await SeedCategoryAsync("Predicate Id");

        var result = await _repository.GetById(c => c.Name == "Predicate Id", CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
    }

    [Fact]
    public async Task Update_Should_Modify_Entity()
    {
        var category = await SeedCategoryAsync("Before Update");
        category.Name = "After Update";

        _repository.Update(category);
        await _context.SaveChangesAsync();

        var stored = await _context.Categories.FindAsync(category.Id);
        stored!.Name.Should().Be("After Update");
    }

    [Fact]
    public async Task UpdateRange_Should_Set_ModifiedDate_And_Update_Entities()
    {
        var first = await SeedCategoryAsync("Update Range 1");
        var second = await SeedCategoryAsync("Update Range 2");
        first.Name = "Updated 1";
        second.Name = "Updated 2";

        _repository.UpdateRange(new List<DomainCategory> { first, second });
        await _context.SaveChangesAsync();

        first.ModifiedDate.Should().NotBeNull();
        second.ModifiedDate.Should().NotBeNull();
        (await _context.Categories.FindAsync(first.Id))!.Name.Should().Be("Updated 1");
        (await _context.Categories.FindAsync(second.Id))!.Name.Should().Be("Updated 2");
    }

    [Fact]
    public async Task AnyAsync_Should_Return_True_When_Entity_Exists()
    {
        await SeedCategoryAsync("Exists");

        var exists = await _repository.AnyAsync(c => c.Name == "Exists", CancellationToken.None);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task Contains_Should_Return_True_For_Tracked_Entity()
    {
        var category = await SeedCategoryAsync("Contains Test");

        var contains = await _repository.Contains(category, CancellationToken.None);

        contains.Should().BeTrue();
    }

    [Fact]
    public void FromSqlRaw_Should_Throw_For_InMemory_Provider()
    {
        var act = () => _repository.FromSqlRaw("SELECT * FROM \"Categories\"").ToList();

        act.Should().Throw<InvalidOperationException>();
    }

    private static DomainCategory CreateCategory(string name, bool isActive = true)
    {
        return new DomainCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test",
            IsActive = isActive,
            CreatedDate = DateTime.UtcNow
        };
    }

    private async Task<DomainCategory> SeedCategoryAsync(string name, bool isActive = true)
    {
        var category = CreateCategory(name, isActive);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
