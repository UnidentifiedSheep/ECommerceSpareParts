using Abstractions.Interfaces.Exceptions;
using Locan.Core.Interfaces.Localizers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public abstract class ExceptionHandlerBase<THandler>(ILogger<THandler> logger) : IExceptionHandler
{
	public abstract ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken);

	protected virtual void LogException(
		HttpContext context,
		Exception exception,
		int statusCode)
	{
		var logLevel = statusCode >= StatusCodes.Status500InternalServerError
			? LogLevel.Error
			: LogLevel.Information;
		if (!logger.IsEnabled(logLevel))
			return;

		using (logger.BeginScope(
					new Dictionary<string, object>
					{
						["TraceId"] = context.TraceIdentifier
					}))
		{
			logger.Log(
				logLevel,
				exception,
				"Request failed with status code {StatusCode} at {Time}",
				statusCode,
				DateTime.UtcNow);
		}
	}

	protected ProblemDetails GetBaseDetails(
		Exception exception,
		HttpContext httpContext,
		int? statusCode = 500)
	{
		return new ProblemDetails
		{
			Title = exception.GetType().Name,
			Detail = exception.Message,
			Status = statusCode,
			Instance = httpContext.Request.Path,
			Type = $"https://httpstatuses.io/{statusCode}",
			Extensions = new Dictionary<string, object?>
			{
				["traceId"] = httpContext.TraceIdentifier
			}
		};
	}

	protected void AddExceptionRelatedData(ProblemDetails problem, Exception ex)
	{
		if (ex is not IValuedException valuedEx)
			return;
		var errorValues = valuedEx.GetErrorValues();
		if (errorValues != null)
			problem.Extensions["errorRelatedData"] = errorValues;
	}

	protected int GetStatusCode(Exception exception)
	{
		if (exception is not IStatusCode scEx)
			return 500;
		return (int)scEx.StatusCode;
	}

	protected bool TryGetLocalizableMessageFromException(
		IContextualLocalizer localizer,
		Exception exception,
		out string? detail)
	{
		detail = null;
		if (exception is not ILocalizableException localizableException)
			return false;

		var message = localizableException.LocalizableMessage;
		if (localizer.TryGet(message, out detail))
			return true;

		logger.LogError("Unable to get localizable message for key: {Key}", message.MessageKey);
		return false;
	}
}
