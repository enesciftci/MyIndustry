using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Integration.Identity;

public class AuthIntegrationTests : IClassFixture<IdentityWebApplicationFactory>
{
    private const string Base = "/api/v1/auth";
    private readonly IdentityWebApplicationFactory _factory;

    public AuthIntegrationTests(IdentityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Should_Create_New_User()
    {
        var client = _factory.CreateClient();
        var email = $"integration-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync($"{Base}/register", new
        {
            email,
            password = "Password1!",
            confirmPassword = "Password1!",
            firstName = "Integration",
            lastName = "User",
            userType = 0
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_With_Invalid_Credentials_Should_Return_Unauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"{Base}/login", new
        {
            email = "nonexistent@example.com",
            password = "WrongPassword1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_With_Valid_Token_Should_Not_Return_500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.GetAsync(Base);

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ForgotPassword_Should_Accept_Request()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"{Base}/forgot-password", new
        {
            email = "integration@example.com",
            clientUrl = "http://localhost:3000"
        });

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task RefreshToken_With_Invalid_Token_Should_Not_Return_500()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"{Base}/refresh-token", new
        {
            refreshToken = "invalid-refresh-token"
        });

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
