using Abstractions.Interfaces.Exceptions;
using Localization.Abstractions;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public abstract class ExceptionHandlerBase<THandler>(
    ILogger<THandler> logger
) : IExceptionHandler
{
    public abstract ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken);

    protected virtual void LogError(HttpContext context, Exception exception)
    {
        if (!logger.IsEnabled(LogLevel.Error)) return;
        using (logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier }))
        {
            logger.LogError(
                exception,
                "Error occurred at {Time}",
                DateTime.UtcNow);
        }
    }

    protected ProblemDetails GetBaseDetails(
        Exception exception,
        HttpContext httpContext,
        int? statusCode = 500)
    {
        return new ProblemDetails
        {
            Title = exception.GetType().Name,
            Detail = exception.Message,
            Status = statusCode,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.io/{statusCode}",
            Extensions = new Dictionary<string, object?>
            {
                ["traceId"] = httpContext.TraceIdentifier
            }
        };
    }

    protected void AddExceptionRelatedData(ProblemDetails problem, Exception ex)
    {
        if (ex is not IValuedException valuedEx) return;
        var errorValues = valuedEx.GetErrorValues();
        if (errorValues != null) problem.Extensions["errorRelatedData"] = errorValues;
    }

    protected int GetStatusCode(Exception exception)
    {
        if (exception is not IStatusCode scEx) return 500;
        return (int)scEx.StatusCode;
    }

    protected bool TryGetLocalizableMessageFromException(
        IScopedStringLocalizer localizer,
        Exception exception,
        out string? detail)
    {
        detail = null;
        if (exception is not ILocalizableException localizableException) return false;

        var key = localizableException.MessageKey;
        if (!localizer.TryGet(key, out var message) || message == null)
            logger.LogError("Unable to get localizable message for Key: {Key}", key);

        if (LocalizedMessageFormatter.TryFormat(
                message!,
                localizableException.Arguments,
                out detail))
            return true;

        logger.LogError(
            "Unable to format localizable message for Key: {Key}, Arguments: {@Args}",
            key,
            localizableException.Arguments);
        return false;
    }
}