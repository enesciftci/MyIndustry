using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class AdminControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/admin";
    private readonly ApiWebApplicationFactory _factory;

    public AdminControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetStats_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.GetAsync($"{Base}/stats");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetListings_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.GetAsync($"{Base}/listings");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task ApproveListing_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new { approve = true });
        var response = await client.PostAsync($"{Base}/listings/{Guid.NewGuid()}/approve", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SuspendListing_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new { suspend = true });
        var response = await client.PostAsync($"{Base}/listings/{Guid.NewGuid()}/suspend", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SuspendSeller_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new { suspend = true });
        var response = await client.PostAsync($"{Base}/sellers/{Guid.NewGuid()}/suspend", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
