using Core.Exceptions;
using Core.Interfaces;
using Exceptions.Base;
using Exceptions.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Api.Common.ExceptionHandlers
{
	public class CustomExceptionHandler : IExceptionHandler
	{
		public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
		{
			Log.ForContext("traceId", context.TraceIdentifier)
				.Error(exception, "Error occurred at {time}", DateTime.UtcNow);

			var showDetails = exception is ValidationException or 
				BadRequestException or 
				UnauthorizedAccessException or 
				NotFoundException or
				ConflictException or
				PreconditionRequiredException;

			var statusCode = exception switch
			{
				InternalServerException => StatusCodes.Status500InternalServerError,
				ValidationException => StatusCodes.Status400BadRequest,
				BadRequestException => StatusCodes.Status400BadRequest,
				UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
				NotFoundException => StatusCodes.Status404NotFound,
				ConflictException => StatusCodes.Status409Conflict,
				PreconditionRequiredException => StatusCodes.Status428PreconditionRequired,
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
			
			if (exception is IValuedException valuedEx)
			{
				var errorValues = valuedEx.GetErrorValues();
				if (errorValues != null)
					problemDetails.Extensions["errorRelatedData"] = errorValues;
			}

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
