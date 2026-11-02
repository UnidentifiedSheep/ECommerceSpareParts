using BulkValidation.Core.Exceptions;
using Exceptions.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GraphQL.Common.ErrorFilters;

public sealed class DbValidationErrorFilter(
	ILoggerFactory loggerFactory,
	IContextualLocalizer localizer,
	IHttpContextAccessor httpContextAccessor)
	: GraphQlErrorFilterBase<DbValidationErrorFilter, ValidationException>(
		loggerFactory, localizer, httpContextAccessor)
{
	protected override IError Handle(IError error, ValidationException exception)
	{
		var failures = new List<IReadOnlyDictionary<string, object?>>();
		foreach (var failure in exception.Failures)
		{
			var detail = failure.Message;
			if (failure.Error is ILocalizableException localized && Localizer.TryGet(localized.LocalizableMessage, out var message))
				detail = message;

			var item = new Dictionary<string, object?>
			{
				["title"] = failure.Error?.GetType().Name ?? failure.ErrorName ?? "An unexpected error occurred",
				["detail"] = detail,
				["status"] = failure.Error is IStatusCode status ? (int)status.StatusCode : StatusCodes.Status500InternalServerError
			};
			if (failure.Error is IValuedException valued && valued.GetErrorValues() is { } values)
				item["errorRelatedData"] = values;
			failures.Add(item);
		}

		var maximumStatus = failures.Select(item => (int)item["status"]!)
			.DefaultIfEmpty(StatusCodes.Status500InternalServerError).Max();
		return CreateErrorBuilder(error, exception, exception.Message, "DB_VALIDATION_ERROR", maximumStatus)
			.SetExtension("errors", failures).Build();
	}
}
