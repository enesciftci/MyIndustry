using MyIndustry.Container.Logging;
using MyIndustry.Container.Middleware;
using Serilog;
using Serilog.Formatting.Compact;

namespace MyIndustry.Container.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder ConfigureMyIndustrySerilog(this WebApplicationBuilder builder, string applicationName)
    {
        builder.Host.UseSerilog((context, _, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("MassTransit", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(new RenderedCompactJsonFormatter()));

        return builder;
    }

    public static IHostBuilder ConfigureMyIndustrySerilog(this IHostBuilder hostBuilder, string applicationName)
    {
        hostBuilder.UseSerilog((context, _, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("MassTransit", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(new RenderedCompactJsonFormatter()));

        return hostBuilder;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();

    public static IApplicationBuilder UseMyIndustryExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();

    public static IApplicationBuilder UseMyIndustryRequestLogging(this IApplicationBuilder app) =>
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.Items.TryGetValue(CorrelationIdConstants.ItemKey, out var correlationId))
                    diagnosticContext.Set("CorrelationId", correlationId);
            };
        });
}
