using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class ServiceControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/services";
    private readonly ApiWebApplicationFactory _factory;

    public ServiceControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        using var content = new MultipartFormDataContent
        {
            { new StringContent("Smoke Service"), "title" },
            { new StringContent("Description"), "description" },
            { new StringContent("100"), "price" },
            { new StringContent("7"), "estimatedDay" },
            { new StringContent(Guid.NewGuid().ToString()), "categoryId" }
        };
        var response = await client.PostAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetServicesBySellerId_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var response = await client.GetAsync($"{Base}/list?index=1&size=10");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetServicesByRandomly_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/list/randomly?index=1&size=10");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Update_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        using var content = new MultipartFormDataContent
        {
            { new StringContent("Updated"), "title" },
            { new StringContent("Updated desc"), "description" },
            { new StringContent("200"), "price" },
            { new StringContent("14"), "estimatedDay" }
        };
        var response = await client.PutAsync($"{Base}/{Guid.NewGuid()}", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetServiceById_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetServiceBySlug_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/slug/smoke-test-slug");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetServiceByFilter_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/filter?index=1&size=10");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task PatchService_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var content = JsonContent.Create(new { reactivateOrExtendExpiry = false });
        var response = await client.PatchAsync($"{Base}/{Guid.NewGuid()}", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Delete_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateSellerToken());
        var response = await client.DeleteAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task IncreaseViewCount_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.PostAsync($"{Base}/increase-viewcount/{Guid.NewGuid()}", null);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Search_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/search?query=test&index=1&size=10");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
