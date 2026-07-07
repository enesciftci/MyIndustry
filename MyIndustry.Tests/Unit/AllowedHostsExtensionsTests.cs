using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using MyIndustry.Container.Extensions;

namespace MyIndustry.Tests.Unit;

public class AllowedHostsExtensionsTests
{
    [Fact]
    public void ValidateProductionAllowedHosts_ProductionWithWildcard_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = "*" })
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var act = () => AllowedHostsExtensions.ValidateProductionAllowedHosts(config, environment.Object);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AllowedHosts must be set to specific domains in Production*");
    }

    [Fact]
    public void ValidateProductionAllowedHosts_ProductionWithEmpty_Throws()
    {
        var config = new ConfigurationBuilder().Build();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var act = () => AllowedHostsExtensions.ValidateProductionAllowedHosts(config, environment.Object);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValidateProductionAllowedHosts_ProductionWithSpecificHosts_DoesNotThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = "api.myindustry.com" })
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var act = () => AllowedHostsExtensions.ValidateProductionAllowedHosts(config, environment.Object);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateProductionAllowedHosts_DevelopmentWithWildcard_DoesNotThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedHosts"] = "*" })
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var act = () => AllowedHostsExtensions.ValidateProductionAllowedHosts(config, environment.Object);

        act.Should().NotThrow();
    }
}
