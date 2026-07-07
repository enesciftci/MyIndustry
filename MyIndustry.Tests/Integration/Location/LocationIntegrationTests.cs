using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyIndustry.ApplicationService.Helpers;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.DbContext;
using MyIndustry.Tests.Fixtures;
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

        var normalizedQuery = SearchTermHelper.NormalizeForSearch("istan");
        var cities = (await _context.Cities.Where(c => c.IsActive).ToListAsync())
            .Where(c => SearchTermHelper.NormalizeForSearch(c.Name).Contains(normalizedQuery))
            .OrderBy(c => c.Name)
            .Take(10)
            .ToList();

        cities.Should().HaveCount(1);
        cities[0].Name.Should().Be("İstanbul");
    }

    [Fact]
    public async Task SearchDistricts_With_CityFilter_Should_Scope_Results()
    {
        var (city, district, _) = await TestDataBuilder.SeedLocationHierarchyAsync(_context);

        var normalizedQuery = SearchTermHelper.NormalizeForSearch("kad");
        var districts = (await _context.Districts
                .Where(d => d.IsActive && d.CityId == city.Id)
                .ToListAsync())
            .Where(d => SearchTermHelper.NormalizeForSearch(d.Name).Contains(normalizedQuery))
            .OrderBy(d => d.Name)
            .Take(20)
            .ToList();

        districts.Should().HaveCount(1);
        districts[0].Id.Should().Be(district.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

public class LocationSearchApiIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/locations";
    private readonly ApiWebApplicationFactory _factory;

    public LocationSearchApiIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SearchCities_Via_Api_Should_Return_Istanbul_For_Istan_Query()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/cities/search?query=istan");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("success").GetBoolean().Should().BeTrue();

        var cities = json.GetProperty("cities");
        cities.GetArrayLength().Should().Be(1);
        cities[0].GetProperty("name").GetString().Should().Be("İstanbul");
    }

    [Fact]
    public async Task SearchDistricts_Via_Api_Should_Return_Kadikoy_For_Kad_Query()
    {
        var client = _factory.CreateSeededClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyIndustryDbContext>();
        var city = await db.Cities.FirstAsync(c => c.Name == "İstanbul");
        var district = await db.Districts.FirstAsync(d => d.CityId == city.Id);

        var response = await client.GetAsync($"{Base}/districts/search?query=kad&cityId={city.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var districts = json.GetProperty("districts");
        districts.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        districts.EnumerateArray().Select(d => d.GetProperty("id").GetGuid())
            .Should().Contain(district.Id);
    }
}
