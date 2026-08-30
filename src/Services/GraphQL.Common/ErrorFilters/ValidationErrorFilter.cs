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
		var validationErrors = new List<ValidationErrorModel>();

		foreach (var failure in exception.Errors)
		{
			var state = failure.CustomState as ValidationStateData;
			if (!(state?.DisplayErrorToUser ?? true))
				continue;

			var message = failure.ErrorMessage;
			if (!string.IsNullOrWhiteSpace(failure.ErrorCode))
				message = state?.ErrorMessageArguments is { Length: > 0 } arguments
					? Localizer.GetOrDefault(failure.ErrorCode, arguments) ?? message
					: Localizer.GetOrDefault(failure.ErrorCode) ?? message;

			validationErrors.Add(
				new ValidationErrorModel(
					failure.PropertyName,
					message,
					failure.AttemptedValue));
		}

		return CreateErrorBuilder(
				error,
				exception,
				exception.Message,
				"VALIDATION_ERROR",
				StatusCodes.Status400BadRequest)
			.SetExtension("validationErrors", validationErrors)
			.Build();
	}
}
