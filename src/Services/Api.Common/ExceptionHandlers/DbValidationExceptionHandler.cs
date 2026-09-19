using BulkValidation.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public class DbValidationExceptionHandler(ILogger<DbValidationExceptionHandler> logger)
	: ExceptionHandlerBase<DbValidationExceptionHandler>(logger)
{
	public override async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		if (exception is not ValidationException dbValidationException)
			return false;

		var problemDetails = GetBaseDetails(
			dbValidationException,
			httpContext,
			null);
		SetStatusCode(problemDetails, dbValidationException);
		AddDbValidationErrors(problemDetails, dbValidationException);
		LogException(
			httpContext,
			exception,
			problemDetails.Status ?? StatusCodes.Status500InternalServerError);

		httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
		return true;
	}

	private void AddDbValidationErrors(ProblemDetails details, ValidationException bulkEx)
	{
		var errors = new List<ProblemDetails>();

		foreach (var fail in bulkEx.Failures)
		{
			var errorName = fail.ErrorName ?? "An unexpected error occurred";
			var errorCode = fail.ErrorCode;
			// TODO: Replace the raw message with ILocalizableMessage when BulkValidation exposes it.

			errors.Add(
				new ProblemDetails
				{
					Title = errorName,
					Detail = fail.Message,
					Status = errorCode
				});
		}

		details.Extensions["errors"] = errors;
	}

	private void SetStatusCode(ProblemDetails details, ValidationException bulkEx)
	{
		var max = -1;

		foreach (var failure in bulkEx.Failures)
			if (failure.ErrorCode.HasValue && failure.ErrorCode.Value > max)
				max = failure.ErrorCode.Value;

		details.Status = max == -1 ? StatusCodes.Status500InternalServerError : max;
	}
}
