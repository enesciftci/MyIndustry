using MyIndustry.Tests.Fixtures;

namespace MyIndustry.Tests.Smoke;

public class LocationControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/locations";
    private readonly ApiWebApplicationFactory _factory;

    public LocationControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCities_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/cities");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetDistricts_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/cities/{Guid.NewGuid()}/districts");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetNeighborhoods_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/districts/{Guid.NewGuid()}/neighborhoods");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SearchCities_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/cities/search?query=ist");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SearchDistricts_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/districts/search?query=mer");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SearchNeighborhoods_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/neighborhoods/search?query=mer");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
