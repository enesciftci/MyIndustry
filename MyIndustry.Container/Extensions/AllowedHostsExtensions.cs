using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MyIndustry.Container.Extensions;

public static class AllowedHostsExtensions
{
    public static void ValidateProductionAllowedHosts(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
            return;

        var allowedHosts = configuration["AllowedHosts"];
        if (string.IsNullOrWhiteSpace(allowedHosts) || allowedHosts == "*" || allowedHosts.Contains('*'))
        {
            throw new InvalidOperationException(
                "AllowedHosts must be set to specific domains in Production " +
                "(e.g. AllowedHosts=api.myindustry.com;gateway.myindustry.com or AllowedHosts__0=yourdomain.com).");
        }
    }
}
