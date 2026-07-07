using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class AuthControllerSmokeTests : IClassFixture<IdentityWebApplicationFactory>
{
    private const string Base = "/api/v1/auth";
    private readonly IdentityWebApplicationFactory _factory;

    public AuthControllerSmokeTests(IdentityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { email = "smoke@example.com", password = "Password1!" });
        var response = await client.PostAsync($"{Base}/login", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Logout_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.PostAsync($"{Base}/logout", null);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task RefreshToken_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { refreshToken = "invalid-token" });
        var response = await client.PostAsync($"{Base}/refresh-token", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Register_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new
        {
            email = $"smoke-{Guid.NewGuid():N}@example.com",
            password = "Password1!",
            confirmPassword = "Password1!",
            firstName = "Smoke",
            lastName = "User",
            userType = 0
        });
        var response = await client.PostAsync($"{Base}/register", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Verify_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { email = "smoke@example.com", code = "000000" });
        var response = await client.PostAsync($"{Base}/verify", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task ConfirmEmail_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { userId = Guid.NewGuid().ToString(), token = "invalid" });
        var response = await client.PostAsync($"{Base}/confirm-email", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task ConfirmEmailByCode_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { email = "smoke@example.com", code = "000000" });
        var response = await client.PostAsync($"{Base}/confirm-email-by-code", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task ResendVerificationEmail_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { email = "smoke@example.com" });
        var response = await client.PostAsync($"{Base}/resend-verification-email", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { email = "smoke@example.com", clientUrl = "http://localhost:3000" });
        var response = await client.PostAsync($"{Base}/forgot-password", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task ResetPassword_ReturnsNon500()
    {
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new
        {
            userId = Guid.NewGuid().ToString(),
            token = "invalid",
            newPassword = "NewPassword1!"
        });
        var response = await client.PostAsync($"{Base}/reset-password", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task Get_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.GetAsync(Base);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SendPhoneVerification_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new { phoneNumber = "+905551234567" });
        var response = await client.PostAsync($"{Base}/send-phone-verification", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task VerifyPhone_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new { code = "000000" });
        var response = await client.PostAsync($"{Base}/verify-phone", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task SendEmailChangeVerification_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new { newEmail = "new@example.com" });
        var response = await client.PostAsync($"{Base}/send-email-change-verification", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task VerifyEmailChange_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new { code = "000000" });
        var response = await client.PostAsync($"{Base}/verify-email-change", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task UpdateProfile_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new { firstName = "Smoke", lastName = "User" });
        var response = await client.PutAsync($"{Base}/profile", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
