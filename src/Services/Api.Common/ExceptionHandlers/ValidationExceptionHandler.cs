using Abstractions.Models.Validation;
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
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private void AddValidationErrors(HttpContext httpContext, ProblemDetails problemDetails, ValidationException exception)
    {
        var localizer = httpContext.RequestServices.GetService<IScopedStringLocalizer>();
        problemDetails.Extensions["validationErrors"] = exception.Errors
            .Select(e =>
            {
                if (localizer == null || string.IsNullOrWhiteSpace(e.ErrorCode)) 
                    return new ValidationErrorModel(e.PropertyName, e.ErrorMessage, e.AttemptedValue);

                return new ValidationErrorModel(e.PropertyName, localizer[e.ErrorCode], e.AttemptedValue);
            });
    }
}