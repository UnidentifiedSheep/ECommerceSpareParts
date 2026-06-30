using Abstractions.Models.Validation;
using Exceptions;
using FluentValidation;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public class ValidationExceptionHandler(
    ILogger<ValidationExceptionHandler> logger
) : ExceptionHandlerBase<ValidationExceptionHandler>(logger)
{
    public override async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException) return false;

        LogError(httpContext, exception);

        var problemDetails = GetBaseDetails(validationException, httpContext, 400);
        AddValidationErrors(httpContext, problemDetails, validationException);

        httpContext.Response.StatusCode = 400;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private void AddValidationErrors(
        HttpContext httpContext,
        ProblemDetails problemDetails,
        ValidationException exception)
    {
        var localizer = httpContext.RequestServices.GetService<IScopedStringLocalizer>();
        var errors = new List<ValidationErrorModel>();

        foreach (var error in exception.Errors)
        {
            var state = error.CustomState as ValidationStateData;
            if (!(state?.DisplayErrorToUser ?? true))
                continue;

            var errorCode = error.ErrorCode;
            var propertyName = error.PropertyName;
            var errorMessage = error.ErrorMessage;
            var attemptedValue = error.AttemptedValue;

            if (localizer == null || string.IsNullOrWhiteSpace(errorCode))
            {
                errors.Add(new ValidationErrorModel(propertyName, errorMessage, attemptedValue));
                continue;
            }

            var message = state?.ErrorMessageArguments is { Length: > 0 } arguments
                ? localizer.Get(errorCode, arguments)
                : localizer[errorCode];

            errors.Add(new ValidationErrorModel(propertyName, message, attemptedValue));
        }

        problemDetails.Extensions["validationErrors"] = errors;
    }
}
