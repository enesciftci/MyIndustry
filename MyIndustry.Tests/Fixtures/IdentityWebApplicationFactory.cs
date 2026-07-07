using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MyIndustry.Container.Extensions;
using MyIndustry.Identity.Repository;
using RabbitMqCommunicator;
using RedisCommunicator;

namespace MyIndustry.Tests.Fixtures;

public class IdentityWebApplicationFactory : WebApplicationFactory<MyIndustry.Identity.Api.Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "DevelopmentSigningKey-Min32CharsLong-ChangeInProduction",
                ["Jwt:Issuer"] = JwtExtensions.DefaultIssuer,
                ["IdentityUrl"] = "http://localhost",
                ["ConnectionStrings:MyIndustryIdentityDb"] = "Testing",
                ["InternalApi:ApiKey"] = "test-internal-api-key",
                ["MediatRLogging:Enabled"] = "false",
                ["RateLimiting:AuthPermitLimit"] = "10000",
                ["RateLimiting:AuthWindowMinutes"] = "1"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<MyIndustryIdentityDbContext>>();
            services.RemoveAll<MyIndustryIdentityDbContext>();
            services.AddDbContext<MyIndustryIdentityDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            var redisMock = new Mock<IRedisCommunicator>();
            redisMock.Setup(r => r.GetCacheValueAsync<string>(It.IsAny<string>()))
                .ReturnsAsync((string?)null);
            redisMock.Setup(r => r.SetCacheValueAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);
            redisMock.Setup(r => r.DeleteValue(It.IsAny<string>())).Returns(true);
            services.RemoveAll<IRedisCommunicator>();
            services.AddSingleton(redisMock.Object);

            var publisherMock = new Mock<ICustomMessagePublisher>();
            services.RemoveAll<ICustomMessagePublisher>();
            services.AddSingleton(publisherMock.Object);
        });
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
