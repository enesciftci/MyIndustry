using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class SellerControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/sellers";
    private readonly ApiWebApplicationFactory _factory;

    public SellerControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var content = JsonContent.Create(new { title = "Smoke Corp", description = "Test seller", identityNumber = "12345678901" });
        var response = await client.PostAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetList_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/list?index=1&size=10");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetById_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Update_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var content = JsonContent.Create(new { title = "Updated Corp", description = "Updated", identityNumber = "12345678901" });
        var response = await client.PutAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetProfile_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var response = await client.GetAsync($"{Base}/profile");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task UpdateProfile_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var content = JsonContent.Create(new { title = "Profile Corp", description = "Profile update" });
        var response = await client.PutAsync($"{Base}/profile", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
