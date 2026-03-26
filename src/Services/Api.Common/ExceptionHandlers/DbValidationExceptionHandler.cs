using BulkValidation.Core.Exceptions;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public class DbValidationExceptionHandler(
    ILogger<DbValidationExceptionHandler> logger
) : ExceptionHandlerBase<DbValidationExceptionHandler>(logger)
{
    public override async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException dbValidationException) return false;

        LogError(httpContext, exception);

        var problemDetails = GetBaseDetails(dbValidationException, httpContext, null);
        SetStatusCode(problemDetails, dbValidationException);
        AddDbValidationErrors(
            httpContext,
            problemDetails,
            dbValidationException);

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private void AddDbValidationErrors(HttpContext httpContext, ProblemDetails details, ValidationException bulkEx)
    {
        var localizer = httpContext.RequestServices.GetService<IScopedStringLocalizer>();

        details.Extensions["errors"] = bulkEx.Failures
            .Select(errorValue => new ProblemDetails
            {
                Title = errorValue.ErrorName ?? "An unexpected error occurred",
                Detail = localizer != null ? localizer[errorValue.Message] : errorValue.Message,
                Status = errorValue.ErrorCode
            })
            .ToList();
    }

    private void SetStatusCode(ProblemDetails details, ValidationException bulkEx)
    {
        var max = -1;

        foreach (var failure in bulkEx.Failures)
            if (failure.ErrorCode.HasValue && failure.ErrorCode.Value > max)
                max = failure.ErrorCode.Value;

        details.Status = max == -1 ? StatusCodes.Status500InternalServerError : max;
    }
}