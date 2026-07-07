using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class SellerSubscriptionControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/sellersubscriptions";
    private readonly ApiWebApplicationFactory _factory;

    public SellerSubscriptionControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSellerSubscription_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var response = await client.GetAsync(Base);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Subscribe_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var content = JsonContent.Create(new { subscriptionPlanId = Guid.NewGuid() });
        var response = await client.PostAsync($"{Base}/subscribe", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Upgrade_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var content = JsonContent.Create(new { subscriptionPlanId = Guid.NewGuid() });
        var response = await client.PostAsync($"{Base}/upgrade", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
