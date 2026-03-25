using BulkValidation.Core.Exceptions;
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
        AddDbValidationErrors(problemDetails, dbValidationException);
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
    
    private void AddDbValidationErrors(ProblemDetails details, ValidationException bulkEx)
    {
        details.Extensions["errors"] = bulkEx.Failures
            .Select(errorValue => new ProblemDetails
            {
                Title = errorValue.ErrorName ?? "An unexpected error occurred", 
                Detail = errorValue.Message,
                Status = errorValue.ErrorCode
            }).ToList();
    }

    private void SetStatusCode(ProblemDetails details, ValidationException bulkEx)
    {
        int max = -1;

        foreach (var failure in bulkEx.Failures)
            if (failure.ErrorCode.HasValue && failure.ErrorCode.Value > max)
                max = failure.ErrorCode.Value;
                
        details.Status = max == -1 ? StatusCodes.Status500InternalServerError : max;
    }
}