using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using MyIndustry.Container.Logging;
using MyIndustry.Container.Middleware;
using MyIndustry.Domain.ExceptionHandling;

namespace MyIndustry.Tests.Unit.Container;

public class ExceptionHandlingMiddlewareTests
{
    private static ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next, bool isDevelopment = true)
    {
        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns(isDevelopment ? Environments.Development : Environments.Production);
        return new ExceptionHandlingMiddleware(next, logger.Object, environment.Object);
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_400_For_BusinessRuleException()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Items[CorrelationIdConstants.ItemKey] = "corr-123";

        var middleware = CreateMiddleware(_ => throw new BusinessRuleException("RULE_01", "Business error", "User friendly"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Contain("application/json");

        context.Response.Body.Position = 0;
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        json.GetProperty("success").GetBoolean().Should().BeFalse();
        json.GetProperty("code").GetString().Should().Be("RULE_01");
        json.GetProperty("message").GetString().Should().Be("Business error");
        json.GetProperty("correlationId").GetString().Should().Be("corr-123");
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_401_For_UnauthorizedAccessException()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = CreateMiddleware(_ => throw new UnauthorizedAccessException());

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_500_With_Details_In_Development()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";

        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("Unexpected"), isDevelopment: true);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.Body.Position = 0;
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        json.GetProperty("success").GetBoolean().Should().BeFalse();
        json.GetProperty("error").GetProperty("type").GetString().Should().Be("InvalidOperationException");
        json.GetProperty("error").GetProperty("message").GetString().Should().Be("Unexpected");
    }

    [Fact]
    public async Task InvokeAsync_Should_Return_Generic_500_In_Production()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";

        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("Unexpected"), isDevelopment: false);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        context.Response.Body.Position = 0;
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
        json.GetProperty("success").GetBoolean().Should().BeFalse();
        json.GetProperty("message").GetString().Should().Be("Beklenmeyen bir hata oluştu.");
        json.TryGetProperty("error", out _).Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_Should_Call_Next_When_No_Exception()
    {
        var context = new DefaultHttpContext();
        var called = false;
        var middleware = CreateMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
    }
}
