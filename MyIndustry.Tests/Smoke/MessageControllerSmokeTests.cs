using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class MessageControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/messages";
    private readonly ApiWebApplicationFactory _factory;

    public MessageControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SendMessage_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new { serviceId = Guid.NewGuid(), content = "Smoke test message" });
        var response = await client.PostAsync(Base, content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetConversations_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.GetAsync($"{Base}/conversations");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetConversationMessages_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.GetAsync($"{Base}/conversation/{Guid.NewGuid()}/{Guid.NewGuid()}");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task ReplyMessage_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var content = JsonContent.Create(new
        {
            serviceId = Guid.NewGuid(),
            receiverId = Guid.NewGuid(),
            content = "Smoke reply"
        });
        var response = await client.PostAsync($"{Base}/reply", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.PutAsync($"{Base}/read/{Guid.NewGuid()}/{Guid.NewGuid()}", null);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateCustomerToken());
        var response = await client.GetAsync($"{Base}/unread-count");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
