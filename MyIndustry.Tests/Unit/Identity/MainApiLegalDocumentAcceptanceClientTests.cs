using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MyIndustry.Identity.Api.Services;

namespace MyIndustry.Tests.Unit.Identity;

public class MainApiLegalDocumentAcceptanceClientTests
{
    [Fact]
    public async Task SaveUserLegalDocumentAcceptancesAsync_Should_Skip_When_No_Document_Ids()
    {
        var handler = new StubHttpMessageHandler();
        var client = new HttpClient(handler);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MainApiUrl"] = "http://localhost",
                ["InternalApiKey"] = "key"
            })
            .Build();

        var sut = new MainApiLegalDocumentAcceptanceClient(client, config);

        await sut.SaveUserLegalDocumentAcceptancesAsync(Guid.NewGuid(), new List<Guid>());

        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task SaveUserLegalDocumentAcceptancesAsync_Should_Post_When_Configured()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MainApiUrl"] = "http://localhost",
                ["InternalApiKey"] = "test-key"
            })
            .Build();

        var sut = new MainApiLegalDocumentAcceptanceClient(client, config);
        var userId = Guid.NewGuid();
        var docIds = new List<Guid> { Guid.NewGuid() };

        await sut.SaveUserLegalDocumentAcceptancesAsync(userId, docIds);

        handler.RequestCount.Should().Be(1);
        handler.LastRequest!.Headers.GetValues("X-Internal-Api-Key").First().Should().Be("test-key");
    }

    [Fact]
    public async Task SaveUserLegalDocumentAcceptancesAsync_Should_Skip_When_Config_Missing()
    {
        var handler = new StubHttpMessageHandler();
        var client = new HttpClient(handler);
        var config = new ConfigurationBuilder().Build();
        var sut = new MainApiLegalDocumentAcceptanceClient(client, config);

        await sut.SaveUserLegalDocumentAcceptancesAsync(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

        handler.RequestCount.Should().Be(0);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public StubHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
        }

        public int RequestCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }
    }
}
