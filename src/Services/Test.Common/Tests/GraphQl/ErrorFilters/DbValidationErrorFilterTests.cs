using BulkValidation.Core.Models;
using Exceptions;
using Exceptions.Base;
using FluentAssertions;
using GraphQL.Common.ErrorFilters;
using Locan.Core.LocalizableMessages;
using Microsoft.Extensions.Logging.Abstractions;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.Tests.GraphQl.ErrorFilters;

public class DbValidationErrorFilterTests
{
	[Fact]
	public void OnError_ReadsLocalizedMessageAndStatusFromErrorObject()
	{
		var payload = new InvalidInputException(new LocalizableMessage("Domain.NotFound"));
		var filter = CreateFilter();
		var exception = new DbValidationException([
			new ValidationFailure("fallback", 42, error: payload),
			new ValidationFailure("conflict fallback", "code", error: new ConflictException("Conflict"))
		]);

		var result = filter.OnError(ErrorFilterTestFactory.CreateError(exception));

		result.Code.Should().Be("DB_VALIDATION_ERROR");
		result.Exception.Should().BeNull();
		result.Extensions!["status"].Should().Be(409);
		var errors = Errors(result);
		errors[0]["title"].Should().Be(nameof(InvalidInputException));
		errors[0]["detail"].Should().Be("Entity 42 was not found");
		errors[0]["status"].Should().Be(400);
		errors[1]["detail"].Should().Be("conflict fallback");
	}

	[Fact]
	public void OnError_PreservesRelatedDataFromErrorObject()
	{
		var values = new { Id = 42 };
		var exception = new DbValidationException([
			new ValidationFailure("missing", 42, error: new NotFoundException("missing", values))
		]);

		var result = CreateFilter().OnError(ErrorFilterTestFactory.CreateError(exception));

		Errors(result)[0]["errorRelatedData"].Should().BeSameAs(values);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void OnError_UsesFallbackForMissingPayloadOrTranslation(bool hasPayload)
	{
		var payload = hasPayload ? new InvalidInputException(new LocalizableMessage("Missing.Translation")) : null;
		var exception = new DbValidationException([new ValidationFailure("fallback", 42, error: payload)]);

		var result = CreateFilter().OnError(ErrorFilterTestFactory.CreateError(exception));

		Errors(result)[0]["detail"].Should().Be("fallback");
		result.Extensions!["status"].Should().Be(hasPayload ? 400 : 500);
	}

	private static IReadOnlyList<IReadOnlyDictionary<string, object?>> Errors(HotChocolate.IError result) =>
		result.Extensions!["errors"].Should().BeAssignableTo<IReadOnlyList<IReadOnlyDictionary<string, object?>>>().Subject;

	private static DbValidationErrorFilter CreateFilter() => new(NullLoggerFactory.Instance,
		ErrorFilterTestFactory.CreateLocalizer(), ErrorFilterTestFactory.CreateHttpContextAccessor());
}
