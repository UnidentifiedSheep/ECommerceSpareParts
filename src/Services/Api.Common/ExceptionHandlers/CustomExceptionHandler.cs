using Api.Common.Extensions;
using Exceptions.Base;
using Exceptions.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Api.Common.ExceptionHandlers;

public class CustomExceptionHandler : IExceptionHandler
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

    private static void LogError(HttpContext context, Exception exception)
    {
        Log.ForContext("traceId", context.TraceIdentifier)
            .Error(exception, "Error occurred at {time}", DateTime.UtcNow);
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
            Extensions =
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
            case GroupedException groupedEx:
                problemDetails.Extensions["errors"] = GetGroupedErrors(groupedEx);
                break;
            case IValuedException valuedEx:
                AddValuedExceptionData(problemDetails, valuedEx);
                break;
            case ValidationException validationEx:
                problemDetails.Extensions["validationErrors"] = validationEx.Errors.Select(e => new
                {
                    propertyName = e.PropertyName,
                    errorMessage = e.ErrorMessage,
                    attemptedValue = e.AttemptedValue
                });
                break;
        }
    }

    private static IEnumerable<ProblemDetails> GetGroupedErrors(GroupedException groupedEx)
    {
        var problems = new List<ProblemDetails>();
        foreach (var errorValue in groupedEx.Exceptions)
        {
            var showDetails = ShowDetails(errorValue);

            var problem = new ProblemDetails
            {
                Title = showDetails ? errorValue.GetType().Name : "An unexpected error occurred",
                Detail = showDetails ? errorValue.Message : null
            };

            if (errorValue is IValuedException valuedEx)
                AddValuedExceptionData(problem, valuedEx);

            problems.Add(problem);
        }

        return problems;
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
            GroupedException;
    }
}
