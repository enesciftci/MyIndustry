using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class LegalDocumentControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/legaldocument";
    private readonly ApiWebApplicationFactory _factory;

    public LegalDocumentControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetActiveForRegistration_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/public/for-registration");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetByType_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/public/by-type/1");
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
    public async Task GetById_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.GetAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Create_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new { title = "Smoke", content = "Test", documentType = 1 });
        var response = await client.PostAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Update_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var content = JsonContent.Create(new { id = Guid.NewGuid(), title = "Smoke", content = "Test", documentType = 1 });
        var response = await client.PutAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Delete_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.DeleteAsync($"{Base}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
