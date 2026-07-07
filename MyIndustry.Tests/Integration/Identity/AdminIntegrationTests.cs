using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Integration.Identity;

public class AdminIntegrationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private const string Base = "/api/v1/admin";
    private readonly IdentityWebApplicationFactory _factory;

    public AdminIntegrationTests(IdentityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_With_Admin_Token_Should_Return_Success()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.GetAsync($"{Base}/users?index=1&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetUsers_Without_Token_Should_Return_Unauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"{Base}/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SuspendUser_With_Admin_Token_Should_Not_Return_500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.PostAsJsonAsync(
            $"{Base}/users/{Guid.NewGuid()}/suspend",
            new { reason = "Integration test suspension" });

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UnsuspendUser_With_Admin_Token_Should_Not_Return_500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.PostAsync($"{Base}/users/{Guid.NewGuid()}/unsuspend", null);

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
