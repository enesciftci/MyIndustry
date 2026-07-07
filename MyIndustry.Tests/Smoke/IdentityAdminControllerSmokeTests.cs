using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class IdentityAdminControllerSmokeTests : IClassFixture<IdentityWebApplicationFactory>
{
    private const string Base = "/api/v1/admin";
    private readonly IdentityWebApplicationFactory _factory;

    public IdentityAdminControllerSmokeTests(IdentityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.GetAsync($"{Base}/users");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SuspendUser_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new { reason = "Smoke test" });
        var response = await client.PostAsync($"{Base}/users/{Guid.NewGuid()}/suspend", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task UnsuspendUser_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.PostAsync($"{Base}/users/{Guid.NewGuid()}/unsuspend", null);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
