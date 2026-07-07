using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MyIndustry.Identity.Repository;
using MyIndustry.Repository.DbContext;
using MyIndustry.Tests.Helpers;
using RedisCommunicator;

namespace MyIndustry.Tests.Fixtures;

public class ApiWebApplicationFactory : WebApplicationFactory<MyIndustry.Api.Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly string _identityDbName = Guid.NewGuid().ToString();
    private bool _seeded;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "DevelopmentSigningKey-Min32CharsLong-ChangeInProduction",
                ["IdentityUrl"] = "http://localhost",
                ["ConnectionStrings:MyIndustry"] = "Testing",
                ["ConnectionStrings:MyIndustryIdentityDb"] = "Testing",
                ["InternalApi:ApiKey"] = "test-internal-api-key",
                ["MediatRLogging:Enabled"] = "false"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            RemoveDbContext<MyIndustryDbContext>(services);
            services.AddScoped<MyIndustryDbContext>(_ =>
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseInMemoryDatabase(_dbName);
                return new MyIndustryDbContext(optionsBuilder.Options);
            });

            RemoveDbContext<MyIndustryIdentityDbContext>(services);
            services.AddDbContext<MyIndustryIdentityDbContext>(options =>
                options.UseInMemoryDatabase(_identityDbName));

            var redisMock = new Mock<IRedisCommunicator>();
            redisMock.Setup(r => r.GetCacheValueAsync<string>(It.IsAny<string>()))
                .ReturnsAsync((string?)null);
            redisMock.Setup(r => r.SetCacheValueAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);
            services.RemoveAll<IRedisCommunicator>();
            services.AddSingleton(redisMock.Object);
        });
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateSeededClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public HttpClient CreateSeededClient()
    {
        var client = CreateClient();
        EnsureSeeded();
        return client;
    }

    private void EnsureSeeded()
    {
        if (_seeded) return;
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyIndustryDbContext>();
        TestDataBuilder.SeedSellerWithActiveSubscriptionAsync(db).GetAwaiter().GetResult();
        TestDataBuilder.SeedLocationHierarchyAsync(db).GetAwaiter().GetResult();
        TestDataBuilder.SeedLegalDocumentAsync(db).GetAwaiter().GetResult();
        _seeded = true;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions)
                        || d.ServiceType == typeof(DbContextOptions<TContext>)
                        || d.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }
}
