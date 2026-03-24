using Abstractions.Models.Validation;
using FluentValidation;
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
        AddValidationErrors(problemDetails, validationException);
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private void AddValidationErrors(ProblemDetails problemDetails, ValidationException exception)
    {
        problemDetails.Extensions["validationErrors"] = exception.Errors
            .Select(e => new ValidationErrorModel(e.PropertyName, e.ErrorMessage, e.AttemptedValue));
    }
}