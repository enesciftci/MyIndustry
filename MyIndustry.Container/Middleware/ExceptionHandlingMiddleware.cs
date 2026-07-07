using System.Security.Claims;
using MyIndustry.Container.Logging;
using MyIndustry.Domain.ExceptionHandling;

namespace MyIndustry.Container.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            var correlationId = GetCorrelationId(context);

            _logger.LogWarning(ex,
                "Unauthorized access. CorrelationId={CorrelationId}, Path={Path}, Method={Method}, UserId={UserId}",
                correlationId, context.Request.Path, context.Request.Method, GetUserId(context));

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Bu işlem için giriş yapmanız gerekiyor.",
                correlationId
            });
        }
        catch (BusinessRuleException ex)
        {
            var actualMessage = ex.Message ?? ((Exception)ex).Message ?? "Bilinmeyen hata";
            var correlationId = GetCorrelationId(context);

            _logger.LogWarning(ex,
                "Business rule exception: Code={Code}, Message={Message}, UserMessage={UserMessage}, CorrelationId={CorrelationId}, Path={Path}, Method={Method}, UserId={UserId}",
                ex.Code, ex.Message, ex.UserMessage, correlationId, context.Request.Path, context.Request.Method,
                GetUserId(context));

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                code = ex.Code ?? "BUSINESS_ERROR",
                message = actualMessage,
                userMessage = ex.UserMessage ?? actualMessage,
                correlationId
            });
        }
        catch (Exception ex)
        {
            var correlationId = GetCorrelationId(context);

            _logger.LogError(ex,
                "Unhandled exception occurred. Type={ExceptionType}, Message={Message}, Path={Path}, Method={Method}, CorrelationId={CorrelationId}, UserId={UserId}",
                ex.GetType().FullName,
                ex.Message,
                context.Request.Path,
                context.Request.Method,
                correlationId,
                GetUserId(context));

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            if (_environment.IsDevelopment())
            {
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    error = new
                    {
                        type = ex.GetType().Name,
                        message = ex.Message,
                        path = context.Request.Path.Value,
                        correlationId
                    }
                });
            }
            else
            {
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Beklenmeyen bir hata oluştu.",
                    correlationId
                });
            }
        }
    }

    private static string? GetCorrelationId(HttpContext context) =>
        context.Items.TryGetValue(CorrelationIdConstants.ItemKey, out var value)
            ? value?.ToString()
            : CorrelationIdContext.Current;

    private static string? GetUserId(HttpContext context) =>
        context.User.FindFirst("uid")?.Value
        ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? context.Request.Headers["UserId"].FirstOrDefault();
}
