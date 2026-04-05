using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public class AnyExceptionHandler(
    ILogger<AnyExceptionHandler> logger
) : ExceptionHandlerBase<AnyExceptionHandler>(logger)
{
    public override async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = GetStatusCode(exception);

        var problemDetails = GetBaseDetails(exception, httpContext, statusCode);
        SetLocalizedDetail(problemDetails, httpContext, exception);
        AddExceptionRelatedData(problemDetails, exception);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private void SetLocalizedDetail(ProblemDetails problemDetails, HttpContext httpContext, Exception exception)
    {
        var localizer = httpContext.RequestServices.GetService<IScopedStringLocalizer>();
        if (localizer == null) return;

        if (TryGetLocalizableMessageFromException(localizer, exception, out var localizedMessage))
            problemDetails.Detail = localizedMessage;
    }
}