using MyIndustry.Domain.ExceptionHandling;
using System.Text.Json;

namespace MyIndustry.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Bir sonraki middleware'e geç
        }
        catch (BusinessRuleException ex)
        {
            // Get the actual message - either from the shadowed property or base class
            var actualMessage = ex.Message ?? ((Exception)ex).Message ?? "Bilinmeyen hata";
            
            _logger.LogWarning(ex, "Business rule exception: Code={Code}, Message={Message}, UserMessage={UserMessage}, BaseMessage={BaseMessage}", 
                ex.Code, ex.Message, ex.UserMessage, ((Exception)ex).Message);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            var response = new
            {
                success = false,
                code = ex.Code ?? "BUSINESS_ERROR",
                message = actualMessage,
                userMessage = ex.UserMessage ?? actualMessage
            };
            
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            // Log full exception details including inner exceptions
            _logger.LogError(ex, 
                "Unhandled exception occurred. Type={ExceptionType}, Message={Message}, Path={Path}, Method={Method}", 
                ex.GetType().FullName,
                ex.Message,
                context.Request.Path,
                context.Request.Method);
            
            // Log inner exception if exists
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, 
                    "Inner exception: Type={InnerType}, Message={InnerMessage}",
                    ex.InnerException.GetType().FullName,
                    ex.InnerException.Message);
            }
            
            // Log stack trace separately for better visibility
            _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            // Always return useful error info (can be disabled in production if needed)
            var response = new
            {
                success = false,
                error = new
                {
                    type = ex.GetType().Name,
                    message = ex.Message,
                    detail = ex.ToString(),
                    innerException = ex.InnerException?.Message,
                    path = context.Request.Path.Value
                }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
