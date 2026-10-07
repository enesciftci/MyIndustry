using System.Net;
using System.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task Api_HealthEndpoint_IncludesServiceIdentificationHeader()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.TryGetValues("X-MyIndustry-Service", out var values));
        Assert.Equal("MyIndustry.Api", values!.Single());
    }

    [Fact]
    public void Api_DataProtection_IsRegisteredInTestingWithoutRedis()
    {
        using var scope = _factory.Services.CreateScope();
        var dataProtection = scope.ServiceProvider.GetService<IDataProtectionProvider>();
        Assert.NotNull(dataProtection);
    }
}
