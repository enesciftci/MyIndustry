using MyIndustry.Tests.Fixtures;
using MyIndustry.Tests.Helpers;

namespace MyIndustry.Tests.Smoke;

public class SEOControllerSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string Base = "/api/v1/seo";
    private readonly ApiWebApplicationFactory _factory;

    public SEOControllerSmokeTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSitemap_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/sitemap.xml");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GetRobotsTxt_ReturnsNon500()
    {
        var client = _factory.CreateSeededClient();
        var response = await client.GetAsync($"{Base}/robots.txt");
        SmokeAssertions.AssertValidSmokeResponse(response);
    }

    [Fact]
    public async Task GenerateSlugs_ReturnsNon500()
    {
        var client = _factory.CreateAuthenticatedClient(TestAuthHelper.CreateAdminToken());
        var response = await client.PostAsync($"{Base}/generate-slugs", null);
        SmokeAssertions.AssertValidSmokeResponse(response);
    }
}
