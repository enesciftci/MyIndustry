using System.Diagnostics;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyIndustry.Container.Logging;

namespace MyIndustry.Container.MediatR;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly MediatRLoggingOptions _options;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IOptions<MediatRLoggingOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return await next();

        var requestName = typeof(TRequest).Name;
        var requestType = requestName.EndsWith("Query", StringComparison.Ordinal) ? "Query" : "Command";
        var sanitizedPayload = _options.LogRequestPayload
            ? SensitiveDataMasker.Sanitize(request, _options)
            : null;

        _logger.LogInformation(
            "MediatR request started: {MediatRRequestName} {MediatRRequestType} {@MediatRRequestPayload}",
            requestName,
            requestType,
            sanitizedPayload);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            stopwatch.Stop();

            LogCompletion(requestName, requestType, response, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex,
                "MediatR request failed: {MediatRRequestName} {MediatRRequestType} {MediatRElapsedMs} {@MediatRRequestPayload} {ExceptionType}",
                requestName,
                requestType,
                stopwatch.ElapsedMilliseconds,
                sanitizedPayload,
                ex.GetType().Name);
            throw;
        }
    }

    private void LogCompletion(string requestName, string requestType, TResponse response, long elapsedMs)
    {
        if (TryGetResponseMetadata(response, out var success, out var messageCode))
        {
            _logger.LogInformation(
                "MediatR request completed: {MediatRRequestName} {MediatRRequestType} {MediatRElapsedMs} {MediatRResponseSuccess} {MediatRResponseMessageCode}",
                requestName,
                requestType,
                elapsedMs,
                success,
                messageCode);
            return;
        }

        _logger.LogInformation(
            "MediatR request completed: {MediatRRequestName} {MediatRRequestType} {MediatRElapsedMs}",
            requestName,
            requestType,
            elapsedMs);
    }

    private static bool TryGetResponseMetadata(TResponse response, out bool? success, out string? messageCode)
    {
        success = null;
        messageCode = null;

        if (response is null)
            return false;

        var type = response.GetType();
        var successProperty = type.GetProperty("Success", BindingFlags.Public | BindingFlags.Instance);
        var messageCodeProperty = type.GetProperty("MessageCode", BindingFlags.Public | BindingFlags.Instance);

        if (successProperty?.PropertyType == typeof(bool))
            success = (bool?)successProperty.GetValue(response);

        if (messageCodeProperty?.PropertyType == typeof(string))
            messageCode = (string?)messageCodeProperty.GetValue(response);

        return success is not null || messageCode is not null;
    }
}
