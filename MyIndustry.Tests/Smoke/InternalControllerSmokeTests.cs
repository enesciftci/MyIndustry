using System.Net.Http.Json;
using MyIndustry.Tests.Fixtures;

namespace MyIndustry.Tests.Smoke;

public class InternalControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/internal";
    private readonly ApiWebApplicationFactory _factory;

    public InternalControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SaveUserLegalDocumentAcceptances_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var content = JsonContent.Create(new { userId = Guid.NewGuid(), legalDocumentIds = Array.Empty<Guid>() });
        var response = await client.PostAsync($"{Base}/user-legal-document-acceptances", content);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
