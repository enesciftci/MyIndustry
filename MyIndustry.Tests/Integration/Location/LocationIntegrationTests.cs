using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.DbContext;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Integration.Location;

public class LocationIntegrationTests : IDisposable
{
    private readonly MyIndustryDbContext _context;

    public LocationIntegrationTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
    }

    [Fact]
    public async Task GetCities_Should_Return_Active_Cities_Ordered_By_PlateCode()
    {
        await TestDataBuilder.SeedLocationHierarchyAsync(_context);
        _context.Cities.Add(new City
        {
            Id = Guid.NewGuid(),
            Name = "Ankara",
            PlateCode = 6,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var cities = await _context.Cities
            .Where(c => c.IsActive)
            .OrderBy(c => c.PlateCode)
            .Select(c => new { c.Id, c.Name, c.PlateCode })
            .ToListAsync();

        cities.Should().HaveCount(2);
        cities[0].Name.Should().Be("Ankara");
        cities[1].Name.Should().Be("İstanbul");
    }

    [Fact]
    public async Task GetDistricts_Should_Return_Districts_For_City()
    {
        var (city, district, _) = await TestDataBuilder.SeedLocationHierarchyAsync(_context);

        var districts = await _context.Districts
            .Where(d => d.CityId == city.Id && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

        districts.Should().HaveCount(1);
        districts[0].Name.Should().Be(district.Name);
    }

    [Fact]
    public async Task GetNeighborhoods_Should_Return_Neighborhoods_For_District()
    {
        var (_, district, neighborhood) = await TestDataBuilder.SeedLocationHierarchyAsync(_context);

        var neighborhoods = await _context.Neighborhoods
            .Where(n => n.DistrictId == district.Id && n.IsActive)
            .OrderBy(n => n.Name)
            .ToListAsync();

        neighborhoods.Should().HaveCount(1);
        neighborhoods[0].Name.Should().Be(neighborhood.Name);
    }

    [Fact]
    public async Task SearchCities_Should_Return_Matching_Results()
    {
        await TestDataBuilder.SeedLocationHierarchyAsync(_context);

        var cities = await _context.Cities
            .Where(c => c.IsActive && c.Name.ToLower().Contains("istan"))
            .OrderBy(c => c.Name)
            .Take(10)
            .ToListAsync();

        cities.Should().HaveCount(1);
        cities[0].Name.Should().Be("İstanbul");
    }

    [Fact]
    public async Task SearchDistricts_With_CityFilter_Should_Scope_Results()
    {
        var (city, district, _) = await TestDataBuilder.SeedLocationHierarchyAsync(_context);

        var districts = await _context.Districts
            .Where(d => d.IsActive && d.CityId == city.Id && d.Name.ToLower().Contains("kad"))
            .OrderBy(d => d.Name)
            .Take(20)
            .ToListAsync();

        districts.Should().HaveCount(1);
        districts[0].Id.Should().Be(district.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
