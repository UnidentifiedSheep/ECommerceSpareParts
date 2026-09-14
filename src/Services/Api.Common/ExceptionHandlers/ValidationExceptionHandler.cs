using Abstractions.Models.Validation;
using FluentValidation;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;
using Microsoft.AspNetCore.Mvc;

namespace Api.Common.ExceptionHandlers;

public class ValidationExceptionHandler(
	ILogger<ValidationExceptionHandler> logger,
	IContextualLocalizer localizer)
	: ExceptionHandlerBase<ValidationExceptionHandler>(logger)
{
	public override async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		if (exception is not ValidationException validationException)
			return false;

		LogException(httpContext, exception, StatusCodes.Status400BadRequest);

		var problemDetails = GetBaseDetails(
			validationException,
			httpContext,
			400);
		AddValidationErrors(
			problemDetails,
			validationException);

		httpContext.Response.StatusCode = 400;
		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
		return true;
	}

	private void AddValidationErrors(
		ProblemDetails problemDetails,
		ValidationException exception)
	{
		var errors = new List<ValidationErrorModel>();

		foreach (var error in exception.Errors)
		{
			var state = error.CustomState as ValidationStateData;
			if (!(state?.DisplayErrorToUser ?? true))
				continue;

			var propertyName = error.PropertyName;
			var errorMessage = error.ErrorMessage;
			var attemptedValue = error.AttemptedValue;

			if (error.CustomState is not ILocalizableMessage localizableMessage ||
				!localizer.TryGet(localizableMessage, out var localizedMessage))
			{
				errors.Add(
					new ValidationErrorModel(
						propertyName,
						errorMessage,
						attemptedValue));
				continue;
			}

			errors.Add(
				new ValidationErrorModel(
					propertyName,
					localizedMessage,
					attemptedValue));
		}

		problemDetails.Extensions["validationErrors"] = errors;
	}
}
