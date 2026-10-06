using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using GraphQL.Common.ErrorFilters;
using HotChocolate;
using Locan.Core.LocalizableMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Stubs;
using Path = HotChocolate.Path;

namespace Tests.Tests.GraphQl.ErrorFilters;

public class ValidationErrorFilterTests
{
	[Fact]
	public void OnError_ShouldReturnLocalizedAndFallbackFailures()
	{
		var visibleFailure = new ValidationFailure("Name", "fallback")
		{
			ErrorCode = "Validation.Required",
			AttemptedValue = "value",
			CustomState = new LocalizableMessage("Validation.Required")
		};
		var fallbackFailure = new ValidationFailure("Secret", "fallback secret");
		var exception = new ValidationException([visibleFailure, fallbackFailure]);
		var loggerFactory = new RecordingLoggerFactory();
		var filter = CreateFilter(loggerFactory);

		var result = filter.OnError(ErrorFilterTestFactory.CreateError(exception));

		result.Code.Should().Be("VALIDATION_ERROR");
		result.Exception.Should().BeNull();
		result.Path.Should().Be(Path.FromList(["field"]));
		result.Locations.Should().ContainSingle().Which.Should().Be(new Location(2, 3));
		result.Extensions.Should().ContainKey("status").WhoseValue.Should().Be(400);
		result.Extensions.Should().ContainKey("traceId").WhoseValue.Should().Be("test-trace-id");

		result.Message.Should().Be("Validation failed");
		var errors = result.Extensions!["validationErrors"]
			.Should()
			.BeAssignableTo<IReadOnlyCollection<IReadOnlyDictionary<string, object?>>>()
			.Subject;
		errors.Should().HaveCount(2);
		var validationError = errors.First();
		validationError["propertyName"].Should().Be("Name");
		validationError["errorMessage"].Should().Be("Localized validation for Name");
		validationError["attemptedValue"].Should().Be("value");
		var fallbackError = errors.Last();
		fallbackError["propertyName"].Should().Be("Secret");
		fallbackError["errorMessage"].Should().Be("fallback secret");
		loggerFactory.LogLevels.Should().ContainSingle().Which.Should().Be(LogLevel.Information);
	}

	[Fact]
	public void OnError_ShouldUseFallback_WhenLocalizationMessageDoesNotExist()
	{
		var failure = new ValidationFailure("Name", "fallback")
		{
			ErrorCode = "Validation.Missing", CustomState = new LocalizableMessage("Validation.Missing")
		};
		var exception = new ValidationException([failure]);
		var filter = CreateFilter();

		var result = filter.OnError(ErrorFilterTestFactory.CreateError(exception));

		var errors = result.Extensions!["validationErrors"]
			.Should()
			.BeAssignableTo<IReadOnlyCollection<IReadOnlyDictionary<string, object?>>>()
			.Subject;
		errors.Should().ContainSingle().Which["errorMessage"].Should().Be("fallback");
	}

	[Fact]
	public void OnError_ShouldNotHandleErrorWithoutException()
	{
		var filter = CreateFilter();
		var error = ErrorBuilder.New().SetMessage("GraphQL validation error").Build();

		var result = filter.OnError(error);

		result.Should().BeSameAs(error);
	}

	private static ValidationErrorFilter CreateFilter(ILoggerFactory? loggerFactory = null) => new(
		loggerFactory ?? NullLoggerFactory.Instance,
		ErrorFilterTestFactory.CreateLocalizer(),
		ErrorFilterTestFactory.CreateHttpContextAccessor());
}
