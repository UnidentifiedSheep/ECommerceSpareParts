using Abstractions.Models.Validation;
using FluentValidation;
using Localization.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GraphQL.Common.ErrorFilters;

public sealed class ValidationErrorFilter(
	ILoggerFactory loggerFactory,
	IContextualStringLocalizer localizer,
	IHttpContextAccessor httpContextAccessor)
	: GraphQlErrorFilterBase<ValidationErrorFilter, ValidationException>(
		loggerFactory,
		localizer,
		httpContextAccessor)
{
	protected override IError Handle(IError error, ValidationException exception)
	{
		var validationErrors = new List<IReadOnlyDictionary<string, object?>>();

		foreach (var failure in exception.Errors)
		{
			var state = failure.CustomState as ValidationStateData;
			if (!(state?.DisplayErrorToUser ?? true))
				continue;

			var message = failure.ErrorMessage;
			if (!string.IsNullOrWhiteSpace(failure.ErrorCode))
				message = state?.ErrorMessageArguments is { Length: > 0 } arguments
					? Localizer.Get(failure.ErrorCode, arguments)
					: Localizer[failure.ErrorCode];

			validationErrors.Add(
				new Dictionary<string, object?>
				{
					["propertyName"] = failure.PropertyName,
					["errorMessage"] = message,
					["attemptedValue"] = failure.AttemptedValue
				});
		}

		return CreateErrorBuilder(
				error,
				exception,
				"Validation failed",
				"VALIDATION_ERROR",
				StatusCodes.Status400BadRequest)
			.SetExtension("validationErrors", validationErrors)
			.Build();
	}
}
