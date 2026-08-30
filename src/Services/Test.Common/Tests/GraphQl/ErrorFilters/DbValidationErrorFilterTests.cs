using BulkValidation.Core.Models;
using FluentAssertions;
using GraphQL.Common.ErrorFilters;
using Microsoft.Extensions.Logging.Abstractions;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.Tests.GraphQl.ErrorFilters;

public class DbValidationErrorFilterTests
{
	[Fact]
	public void OnError_ShouldReturnLocalizedFailuresAndMaximumStatus()
	{
		var exception = new DbValidationException(
			[
				new ValidationFailure("Db.Duplicate", new object[] { "code" }, 409, "Conflict", typeof(Exception)),
				new ValidationFailure("Unknown", null, 422, "Validation", typeof(Exception))
			]);
		var filter = new DbValidationErrorFilter(
			NullLoggerFactory.Instance,
			ErrorFilterTestFactory.CreateLocalizer(),
			ErrorFilterTestFactory.CreateHttpContextAccessor());

		var result = filter.OnError(ErrorFilterTestFactory.CreateError(exception));

		result.Code.Should().Be("DB_VALIDATION_ERROR");
		result.Exception.Should().BeNull();
		result.Extensions.Should().ContainKey("status").WhoseValue.Should().Be(422);
		var errors = result.Extensions!["errors"].Should()
			.BeAssignableTo<IReadOnlyCollection<IReadOnlyDictionary<string, object?>>>()
			.Subject;
		errors.Should().HaveCount(2);
		errors.First()["detail"].Should().Be("Duplicate code");
		errors.Last()["detail"].Should().Be("Unknown");
	}
}
