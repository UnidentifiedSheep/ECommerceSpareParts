using Abstractions.Models.Validation;
using Api.Common.Extensions;
using Api.Common.Models;
using Api.Common.Response;
using Exceptions.Base;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using ValidationException = Exceptions.Base.ValidationException;

namespace Api.Common.ExceptionHandlers;

public class AotExceptionHandler(ILogger<AotExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        LogError(context, exception);

        var statusCode = exception.GetStatusCode();
        context.Response.StatusCode = statusCode;

        var problemDetails = CreateErrorResponse(context, exception, statusCode);

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private void LogError(HttpContext context, Exception exception)
    {
        if (!logger.IsEnabled(LogLevel.Error)) return;
        using (logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier }))
            logger.LogError(exception, "Error occurred at {Time}", DateTime.UtcNow);
    }

    private ErrorResponse CreateErrorResponse(HttpContext context, Exception exception, int statusCode)
    {
        var showDetails = ShowDetails(exception);
        var problemDetails = new ErrorResponse
        {
            Title = showDetails ? exception.GetType().Name : "An unexpected error occurred",
            Detail = showDetails ? exception.Message : null,
            Status = statusCode,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.io/{statusCode}",
            TraceId = context.TraceIdentifier,
        };

        AddExceptionExtensions(problemDetails, exception);
        return problemDetails;
    }

    private static void AddExceptionExtensions(ErrorResponse response, Exception exception)
    {
        switch (exception)
        {
            case BulkValidation.Core.Exceptions.ValidationException bulkEx:
                response.Errors = GetDbValidationErrors(bulkEx).ToList();
                break;
            case ValidationException validationEx:
                response.ValidationErrors = validationEx.Errors.ToList();
                break;
        }
    }

    private static IEnumerable<ErrorResponse> GetDbValidationErrors(
        BulkValidation.Core.Exceptions.ValidationException bulkEx)
    {
        return bulkEx.Failures
            .Select(errorValue => new ErrorResponse
            {
                Title = errorValue.ErrorName ?? "An unexpected error occurred", 
                Detail = errorValue.Message,
                Status = errorValue.ErrorCode ?? 500
            }).ToList();
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
