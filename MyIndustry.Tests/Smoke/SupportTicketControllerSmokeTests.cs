using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class SupportTicketControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/support-tickets";
    private readonly ApiWebApplicationFactory _factory;

    public SupportTicketControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var content = JsonContent.Create(new
        {
            name = "Smoke User",
            email = "smoke@example.com",
            subject = "Smoke test",
            message = "Smoke test message",
            category = 0
        });
        var response = await client.PostAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetAll_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.GetAsync(Base);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Update_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new { status = 0, priority = 0 });
        var response = await client.PutAsync($"{Base}/{Guid.NewGuid()}", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
