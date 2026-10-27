using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyIndustry.Container.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";
    public const string SupportTicketPolicy = "support-ticket";
    public const string ViewCountPolicy = "view-count";

    public static IServiceCollection AddMyIndustryRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authPermitLimit = configuration.GetValue("RateLimiting:AuthPermitLimit", 5);
        var authWindowMinutes = configuration.GetValue("RateLimiting:AuthWindowMinutes", 1);
        var supportPermitLimit = configuration.GetValue("RateLimiting:SupportTicketPermitLimit", 3);
        var supportWindowHours = configuration.GetValue("RateLimiting:SupportTicketWindowHours", 1);
        var viewCountPermitLimit = configuration.GetValue("RateLimiting:ViewCountPermitLimit", 30);
        var viewCountWindowMinutes = configuration.GetValue("RateLimiting:ViewCountWindowMinutes", 1);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                // Serialize manually: WriteAsJsonAsync needs PipeWriter.UnflushedBytes (breaks TestHost).
                var body = JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Çok fazla istek gönderildi. Lütfen daha sonra tekrar deneyin."
                });
                await context.HttpContext.Response.WriteAsync(body, cancellationToken);
            };

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authPermitLimit,
                        Window = TimeSpan.FromMinutes(authWindowMinutes),
                        QueueLimit = 0
                    }));

            options.AddPolicy(SupportTicketPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = supportPermitLimit,
                        Window = TimeSpan.FromHours(supportWindowHours),
                        QueueLimit = 0
                    }));

            options.AddPolicy(ViewCountPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = viewCountPermitLimit,
                        Window = TimeSpan.FromMinutes(viewCountWindowMinutes),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    public static IApplicationBuilder UseMyIndustryRateLimiting(this IApplicationBuilder app) =>
        app.UseRateLimiter();

    private static string GetClientIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
