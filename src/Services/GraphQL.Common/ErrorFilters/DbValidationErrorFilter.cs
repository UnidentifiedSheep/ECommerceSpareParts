using BulkValidation.Core.Exceptions;
using HotChocolate.Execution;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GraphQL.Common.ErrorFilters;

public sealed class DbValidationErrorFilter(
	ILoggerFactory loggerFactory,
	IContextualStringLocalizer localizer,
	IHttpContextAccessor httpContextAccessor)
	: GraphQlErrorFilterBase<DbValidationErrorFilter, ValidationException>(
		loggerFactory,
		localizer,
		httpContextAccessor)
{
	protected override IError Handle(IError error, ValidationException exception)
	{
		var failures = new List<IReadOnlyDictionary<string, object?>>();

		foreach (var failure in exception.Failures)
		{
			var arguments = failure.AttemptedValue switch
			{
				IEnumerable<object?> values => values.Where(x => x is not null).Select(x => x!).ToArray(),
				not null => [failure.AttemptedValue],
				_ => null
			};

			var message = arguments is { Length: > 0 }
				? Localizer.GetOrDefault(failure.Message, arguments) ?? failure.Message
				: Localizer.GetOrDefault(failure.Message) ?? failure.Message;

			failures.Add(
				new Dictionary<string, object?>
				{
					["title"] = failure.ErrorName ?? "An unexpected error occurred",
					["detail"] = message,
					["status"] = failure.ErrorCode
				});
		}

		var status = exception.Failures
			.Where(x => x.ErrorCode.HasValue)
			.Select(x => x.ErrorCode!.Value)
			.DefaultIfEmpty(StatusCodes.Status500InternalServerError)
			.Max();

		return CreateErrorBuilder(
				error,
				exception,
				exception.Message,
				"DB_VALIDATION_ERROR",
				status)
			.SetExtension("errors", failures)
			.Build();
	}
}
