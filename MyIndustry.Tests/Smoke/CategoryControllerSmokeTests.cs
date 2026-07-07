using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;

namespace MyIndustry.Tests.Smoke;

public class CategoryControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/categories";
    private readonly ApiWebApplicationFactory _factory;

    public CategoryControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var content = JsonContent.Create(new { name = "Smoke Category", description = "Test" });
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
    public async Task Update_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var content = JsonContent.Create(new { name = "Updated", description = "Test", isActive = true });
        var response = await client.PutAsync($"{Base}/{Guid.NewGuid()}", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Delete_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.DeleteAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task CreateSubCategory_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var content = JsonContent.Create(new { parentId = Guid.NewGuid(), name = "Sub", description = "Test" });
        var response = await client.PostAsync($"{Base}/subcategory", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetTree_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/tree");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetMainCategories_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/main");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetSubCategories_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
