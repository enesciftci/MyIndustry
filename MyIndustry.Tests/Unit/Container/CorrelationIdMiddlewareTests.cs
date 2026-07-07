using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyIndustry.Container.Logging;
using MyIndustry.Container.Middleware;

namespace MyIndustry.Tests.Unit.Container;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_Use_Existing_Correlation_Id_From_Header()
    {
        const string correlationId = "existing-correlation-id";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdConstants.HeaderName] = correlationId;

        string? capturedId = null;
        var middleware = new CorrelationIdMiddleware(ctx =>
        {
            capturedId = CorrelationIdContext.Current;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        capturedId.Should().Be(correlationId);
        context.Items[CorrelationIdConstants.ItemKey].Should().Be(correlationId);
    }

    [Fact]
    public async Task InvokeAsync_Should_Generate_Correlation_Id_When_Header_Missing()
    {
        var context = new DefaultHttpContext();
        string? capturedId = null;

        var middleware = new CorrelationIdMiddleware(ctx =>
        {
            capturedId = CorrelationIdContext.Current;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        capturedId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(capturedId, out _).Should().BeTrue();
        context.Items[CorrelationIdConstants.ItemKey].Should().Be(capturedId);
    }

    [Fact]
    public async Task InvokeAsync_Should_Restore_Correlation_Scope_After_Request()
    {
        string? duringRequest = null;
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdConstants.HeaderName] = "scope-test-id";

        await new CorrelationIdMiddleware(_ =>
        {
            duringRequest = CorrelationIdContext.Current;
            return Task.CompletedTask;
        }).InvokeAsync(context);

        duringRequest.Should().Be("scope-test-id");
        CorrelationIdContext.Current.Should().BeNull();
    }
}
