using BulkValidation.Core.Exceptions;
using Locan.Core.Interfaces.Localizers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GraphQL.Common.ErrorFilters;

public sealed class DbValidationErrorFilter(
	ILoggerFactory loggerFactory,
	IContextualLocalizer localizer,
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
			// TODO: Replace the raw message with ILocalizableMessage when BulkValidation exposes it.
			failures.Add(
				new Dictionary<string, object?>
				{
					["title"] = failure.ErrorName ?? "An unexpected error occurred",
					["detail"] = failure.Message,
					["status"] = failure.ErrorCode
				});

		var status = exception
			.Failures
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
