using Exceptions.Base;
using Locan.Core.Interfaces.Localizers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public class AnyExceptionHandler(
	ILogger<AnyExceptionHandler> logger,
	IContextualLocalizer localizer)
	: ExceptionHandlerBase<AnyExceptionHandler>(logger)
{
	public override async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		var statusCode = GetStatusCode(exception);
		LogException(httpContext, exception, statusCode);

		var problemDetails = GetBaseDetails(
			exception,
			httpContext,
			statusCode);
		if (statusCode == StatusCodes.Status500InternalServerError)
			problemDetails.Detail = nameof(InternalServerException);

		SetLocalizedDetail(
			problemDetails,
			exception);
		AddExceptionRelatedData(problemDetails, exception);

		httpContext.Response.StatusCode = statusCode;
		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
		return true;
	}

	private void SetLocalizedDetail(
		ProblemDetails problemDetails,
		Exception exception)
	{
		if (TryGetLocalizableMessageFromException(
				localizer,
				exception,
				out var localizedMessage))
			problemDetails.Detail = localizedMessage;
	}
}
