using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class FavoriteControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/favorites";
    private readonly ApiWebApplicationFactory _factory;

    public FavoriteControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddFavorite_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new { serviceId = Guid.NewGuid() });
        var response = await client.PostAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task DeleteFavorite_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.DeleteAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetFavorite_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.GetAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetFavoriteList_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.GetAsync(Base);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
