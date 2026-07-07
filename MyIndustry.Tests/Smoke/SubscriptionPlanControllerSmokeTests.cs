using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class SubscriptionPlanControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/subscriptionplans";
    private readonly ApiWebApplicationFactory _factory;

    public SubscriptionPlanControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateSubscriptionPlan_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var content = JsonContent.Create(new
        {
            name = "Smoke Plan",
            description = "Test",
            subscriptionType = 0,
            monthlyPrice = 0,
            monthlyPostLimit = 3,
            postDurationInDays = 30,
            featuredPostLimit = 0
        });
        var response = await client.PostAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetList_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/list");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetAll_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.GetAsync($"{Base}/all");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task UpdateSubscriptionPlan_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new
        {
            name = "Updated Plan",
            description = "Test",
            subscriptionType = 0,
            monthlyPrice = 0,
            monthlyPostLimit = 3,
            postDurationInDays = 30,
            featuredPostLimit = 0,
            isActive = true
        });
        var response = await client.PutAsync($"{Base}/{Guid.NewGuid()}", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task DeleteSubscriptionPlan_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.DeleteAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
