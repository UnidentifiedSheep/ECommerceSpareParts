using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Core.Exceptions.ExceptionHandlers
{
	public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
	{
		public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
		{
			logger.LogError(
				"Error Message: {exceptionMessage}, Time of occurrence {time}",
				exception.Message, DateTime.UtcNow);

			var showDetails = exception is ValidationException or BadRequestException or UnauthorizedAccessException or NotFoundException;

			var statusCode = exception switch
			{
				InternalServerException => StatusCodes.Status500InternalServerError,
				ValidationException => StatusCodes.Status400BadRequest,
				BadRequestException => StatusCodes.Status400BadRequest,
				UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
				NotFoundException => StatusCodes.Status404NotFound,
				ConflictException => StatusCodes.Status409Conflict,
				_ => StatusCodes.Status500InternalServerError
			};

			context.Response.StatusCode = statusCode;

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

			if (exception is ValidationException validationException)
			{
				problemDetails.Extensions["validationErrors"] = validationException.Errors.Select(e => new
				{
					propertyName = e.PropertyName,
					errorMessage = e.ErrorMessage,
					attemptedValue = e.AttemptedValue
				});
			}

			await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
			return true;
		}
	}

}
