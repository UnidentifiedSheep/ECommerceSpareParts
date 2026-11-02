using BulkValidation.Core.Exceptions;
using Exceptions.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public class DbValidationExceptionHandler(ILogger<DbValidationExceptionHandler> logger, IContextualLocalizer localizer)
	: ExceptionHandlerBase<DbValidationExceptionHandler>(logger)
{
	public override async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		if (exception is not ValidationException dbValidationException)
			return false;

		var errors = new List<ProblemDetails>();
		foreach (var failure in dbValidationException.Failures)
		{
			var detail = failure.Message;
			if (failure.Error is ILocalizableException localized && localizer.TryGet(localized.LocalizableMessage, out var message))
				detail = message;
			var error = new ProblemDetails
			{
				Title = failure.Error?.GetType().Name ?? failure.ErrorName ?? "An unexpected error occurred",
				Detail = detail,
				Status = failure.Error is IStatusCode status ? (int)status.StatusCode : StatusCodes.Status500InternalServerError
			};
			if (failure.Error is IValuedException valued && valued.GetErrorValues() is { } values)
				error.Extensions["errorRelatedData"] = values;
			errors.Add(error);
		}

		var statusCode = errors.Select(error => error.Status!.Value)
			.DefaultIfEmpty(StatusCodes.Status500InternalServerError).Max();
		var problemDetails = GetBaseDetails(exception, httpContext, statusCode);
		problemDetails.Extensions["errors"] = errors;
		LogException(httpContext, exception, statusCode);
		httpContext.Response.StatusCode = statusCode;
		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
		return true;
	}
}
