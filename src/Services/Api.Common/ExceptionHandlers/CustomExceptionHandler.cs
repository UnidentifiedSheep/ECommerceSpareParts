using Abstractions.Interfaces.Exceptions;
using Abstractions.Models.Validation;
using Api.Common.Extensions;
using Exceptions.Base;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ValidationException = FluentValidation.ValidationException;

namespace Api.Common.ExceptionHandlers;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        LogError(context, exception);

        var statusCode = exception.GetStatusCode();
        context.Response.StatusCode = statusCode;

        var problemDetails = CreateProblemDetails(context, exception, statusCode);

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private void LogError(HttpContext context, Exception exception)
    {
        if (!logger.IsEnabled(LogLevel.Error)) return;
        using (logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier }))
            logger.LogError(exception, "Error occurred at {Time}", DateTime.UtcNow);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, int statusCode)
    {
        var showDetails = ShowDetails(exception);
        var problemDetails = new ProblemDetails
        {
            Title = showDetails ? exception.GetType().Name : "An unexpected error occurred",
            Detail = showDetails ? exception.Message : null,
            Status = statusCode,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.io/{statusCode}",
            Extensions = new Dictionary<string, object?>
            {
                ["traceId"] = context.TraceIdentifier
            }
        };

        AddExceptionExtensions(problemDetails, exception);
        return problemDetails;
    }

    private static void AddExceptionExtensions(ProblemDetails problemDetails, Exception exception)
    {
        switch (exception)
        {      
            case IValuedException valuedEx:
                AddValuedExceptionData(problemDetails, valuedEx);
                break;
            case BulkValidation.Core.Exceptions.ValidationException bulkEx:
                problemDetails.Extensions["errors"] = GetDbValidationErrors(bulkEx);
                break;
            case ValidationException validationEx:
                problemDetails.Extensions["validationErrors"] = validationEx.Errors
                    .Select(e => new ValidationErrorModel(e.PropertyName, e.ErrorMessage, e.AttemptedValue));
                break;
        }
    }

    private static IEnumerable<ProblemDetails> GetDbValidationErrors(
        BulkValidation.Core.Exceptions.ValidationException bulkEx)
    {
        return bulkEx.Failures
            .Select(errorValue => new ProblemDetails
            {
                Title = errorValue.ErrorName ?? "An unexpected error occurred", 
                Detail = errorValue.Message,
                Status = errorValue.ErrorCode
            }).ToList();
    }

    private static void AddValuedExceptionData(ProblemDetails problem, IValuedException valuedEx)
    {
        var errorValues = valuedEx.GetErrorValues();
        if (errorValues != null)
            problem.Extensions["errorRelatedData"] = errorValues;
    }

    private static bool ShowDetails(object? exception)
    {
        return exception is ValidationException or
            BadRequestException or
            UnauthorizedAccessException or
            NotFoundException or
            ConflictException or
            PreconditionRequiredException or 
            BulkValidation.Core.Exceptions.ValidationException;
    }
}
