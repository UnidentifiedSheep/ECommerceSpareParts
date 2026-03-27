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
        
        var errors = new List<ProblemDetails>();

        foreach (var fail in bulkEx.Failures)
        {
            var errorName = fail.ErrorName ?? "An unexpected error occurred";
            var errorCode = fail.ErrorCode;
            if (localizer == null)
            {
                errors.Add(new ProblemDetails
                {
                    Title = errorName,
                    Detail = fail.Message,
                    Status = errorCode,
                });
                continue;
            }
            
            string key = fail.Message;
            string template = localizer[key];
            object[]? arguments = null;

            if (fail.AttemptedValue is IEnumerable<object?> args)
                arguments = args
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToArray();
            else if (fail.AttemptedValue != null)
                arguments = [fail.AttemptedValue];

            TryFormatLocalizableMessage(template, arguments, out template);
            
            errors.Add(new ProblemDetails
            {
                Title = errorName,
                Detail = template,
                Status = errorCode,
            });
        }

        details.Extensions["errors"] = errors;
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