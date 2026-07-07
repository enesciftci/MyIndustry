using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MyIndustry.Container.Extensions;

namespace MyIndustry.Tests.Unit;

public class CorsExtensionsTests
{
    [Fact]
    public void AddMyIndustryCors_ProductionWithEmptyOrigins_Throws()
    {
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        var act = () => services.AddMyIndustryCors(config, environment.Object);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cors:AllowedOrigins must be set in Production*");
    }

    [Fact]
    public void AddMyIndustryCors_DevelopmentWithEmptyOrigins_RegistersCors()
    {
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        services.AddMyIndustryCors(config, environment.Object);

        services.Should().Contain(sd => sd.ServiceType.Name.Contains("Cors"));
    }

    [Fact]
    public void AddMyIndustryCors_ProductionWithConfiguredOrigins_RegistersCors()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "https://example.com"
            })
            .Build();
        var services = new ServiceCollection();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        services.AddMyIndustryCors(config, environment.Object);

        services.Should().Contain(sd => sd.ServiceType.Name.Contains("Cors"));
    }
}
