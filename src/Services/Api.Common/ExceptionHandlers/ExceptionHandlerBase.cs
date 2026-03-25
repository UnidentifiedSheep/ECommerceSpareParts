using Abstractions.Interfaces.Exceptions;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public abstract class ExceptionHandlerBase<THandler>(
    ILogger<THandler> logger) : IExceptionHandler
{
    public abstract ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken);
    
    protected virtual void LogError(HttpContext context, Exception exception)
    {
        if (!logger.IsEnabled(LogLevel.Error)) return;
        using (logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier }))
            logger.LogError(exception, "Error occurred at {Time}", DateTime.UtcNow);
    }

    protected ProblemDetails GetBaseDetails(Exception exception, HttpContext httpContext, int? statusCode = 500)
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

        string key = localizableException.MessageKey;
        string message = localizer[key];
        var arguments = localizableException.Arguments;

        if (arguments == null || arguments.Length == 0)
        {
            detail = message;
            return true;
        }
        
        try
        {
            detail = string.Format(message, arguments);
            return true;
        }
        catch (FormatException ex)
        {
            logger.LogError(
                ex,
                "Unable to format localizable message for Key: {Key}, Arguments: {@Args}", 
                key,
                arguments);
            
            detail = $"{message} [Error formatting message]";
            return true;
        }
    }
}