using MyIndustry.Tests.Fixtures;

namespace MyIndustry.Tests.Smoke;

public class HealthCheckSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _apiFactory;

    public HealthCheckSmokeTests(ApiWebApplicationFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task Api_GetHealth_ReturnsNon500()
    {
        var client = _apiFactory.CreateSeededClient();
        var response = await client.GetAsync("/health");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Identity_GetHealth_ReturnsNon500()
    {
        using var identityFactory = new IdentityWebApplicationFactory();
        var client = identityFactory.CreateClient();
        var response = await client.GetAsync("/health");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
