using System.Diagnostics;
using Abstractions.Interfaces.Exceptions;
using HotChocolate.Execution;
using Localization.Abstractions;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GraphQL.Common.ErrorFilters;

public abstract class GraphQlErrorFilterBase<TFilter, TException>(
	ILoggerFactory loggerFactory,
	IContextualStringLocalizer localizer,
	IHttpContextAccessor httpContextAccessor) : IErrorFilter
	where TException : Exception
{
	private readonly ILogger<TFilter> _logger = loggerFactory.CreateLogger<TFilter>();

	protected IContextualStringLocalizer Localizer { get; } = localizer;

	public IError OnError(IError error)
	{
		if (error.Exception is not TException exception)
			return error;

		var handledError = Handle(error, exception);
		var statusCode = handledError.Extensions is not null &&
			handledError.Extensions.TryGetValue("status", out var status) &&
			status is int value
			? value
			: GetStatusCode(exception);
		LogException(exception, statusCode);

		return handledError;
	}

	protected abstract IError Handle(IError error, TException exception);

	protected ErrorBuilder CreateErrorBuilder(
		IError error,
		Exception exception,
		string message,
		string code,
		int status)
	{
		var builder = ErrorBuilder.New().SetMessage(message).SetCode(code);

		if (error.Path is not null)
			builder.SetPath(error.Path);

		if (error.Locations is not null)
			foreach (var location in error.Locations)
				builder.AddLocation(location);

		builder
			.SetExtension("title", exception.GetType().Name)
			.SetExtension("status", status);

		var traceId = httpContextAccessor.HttpContext?.TraceIdentifier ??
			Activity.Current?.TraceId.ToString();
		if (!string.IsNullOrWhiteSpace(traceId))
			builder.SetExtension("traceId", traceId);

		return builder;
	}

	protected int GetStatusCode(Exception exception) =>
		exception is IStatusCode statusCodeException ? (int)statusCodeException.StatusCode : 500;

	protected string GetLocalizedMessage(Exception exception, string fallback)
	{
		if (exception is not ILocalizableException localizableException)
			return fallback;

		var key = localizableException.MessageKey;
		if (!Localizer.TryGet(key, out var message) || message is null)
		{
			_logger.LogError("Unable to get localizable message for Key: {Key}", key);
			return fallback;
		}

		if (LocalizedMessageFormatter.TryFormat(
				message,
				localizableException.Arguments,
				out var localizedMessage))
			return localizedMessage;

		_logger.LogError(
			"Unable to format localizable message for Key: {Key}, Arguments: {@Args}",
			key,
			localizableException.Arguments);
		return fallback;
	}

	protected static void AddExceptionRelatedData(ErrorBuilder builder, Exception exception)
	{
		if (exception is IValuedException valuedException &&
			valuedException.GetErrorValues() is { } errorValues)
			builder.SetExtension("errorRelatedData", errorValues);
	}

	private void LogException(Exception exception, int statusCode)
	{
		var logLevel = statusCode >= StatusCodes.Status500InternalServerError
			? LogLevel.Error
			: LogLevel.Information;
		if (!_logger.IsEnabled(logLevel))
			return;

		var traceId = httpContextAccessor.HttpContext?.TraceIdentifier ??
			Activity.Current?.TraceId.ToString();
		using (_logger.BeginScope(
				new Dictionary<string, object?>
				{
					["TraceId"] = traceId
				}))
		{
			_logger.Log(
				logLevel,
				exception,
				"GraphQL request failed with status code {StatusCode} at {Time}",
				statusCode,
				DateTime.UtcNow);
		}
	}
}
