using System.Net;
using MyIndustry.Tests.Fixtures;

namespace MyIndustry.Tests.Smoke;

/// <summary>
/// Minimal smoke coverage for validating the autonomous AI development loop (see docs/ai-loop.md).
/// </summary>
public class AiLoopSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AiLoopSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Api_HealthEndpoint_ReturnsOk()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
