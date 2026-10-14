using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using MyIndustry.Tests.Fixtures;

namespace MyIndustry.Tests.Unit.Api;

public class DataProtectionConfigurationTests
{
    [Fact]
    public void Testing_environment_resolves_data_protection_without_redis()
    {
        using var factory = new ApiWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var protector = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("test-purpose");

        const string payload = "smoke-payload";
        var protectedBytes = protector.Protect(payload);
        protector.Unprotect(protectedBytes).Should().Be(payload);
    }
}
