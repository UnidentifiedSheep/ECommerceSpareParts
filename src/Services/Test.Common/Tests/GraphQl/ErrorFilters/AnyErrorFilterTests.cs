using System.Net;
using Abstractions.Interfaces.Exceptions;
using FluentAssertions;
using GraphQL.Common.ErrorFilters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Stubs;

namespace Tests.Tests.GraphQl.ErrorFilters;

public class AnyErrorFilterTests
{
	[Fact]
	public void OnError_ShouldMaskInternalException()
	{
		var loggerFactory = new RecordingLoggerFactory();
		var filter = CreateFilter(loggerFactory);
		var sourceError = ErrorFilterTestFactory
			.CreateError(new InvalidOperationException("sensitive message"))
			.SetExtension("stackTrace", "sensitive stack trace");

		var result = filter.OnError(sourceError);

		result.Message.Should().Be("InternalServerException");
		result.Code.Should().Be("INTERNAL_SERVER_ERROR");
		result.Exception.Should().BeNull();
		result.Extensions.Should().ContainKey("status").WhoseValue.Should().Be(500);
		result.Extensions.Should().NotContainKey("stackTrace");
		loggerFactory.LogLevels.Should().ContainSingle().Which.Should().Be(LogLevel.Error);
	}

	[Fact]
	public void OnError_ShouldLocalizeKnownExceptionAndAddRelatedData()
	{
		var loggerFactory = new RecordingLoggerFactory();
		var filter = CreateFilter(loggerFactory);

		var result = filter.OnError(
			ErrorFilterTestFactory.CreateError(new TestNotFoundException(42)));

		result.Message.Should().Be("Entity 42 was not found");
		result.Code.Should().Be(nameof(TestNotFoundException));
		result.Extensions.Should().ContainKey("status").WhoseValue.Should().Be(404);
		result.Extensions.Should().ContainKey("errorRelatedData").WhoseValue.Should().Be(42);
		loggerFactory.LogLevels.Should().ContainSingle().Which.Should().Be(LogLevel.Information);
	}

	[Fact]
	public void OnError_ShouldNotRewriteErrorHandledBySpecificFilter()
	{
		var validationFilter = new ValidationErrorFilter(
			NullLoggerFactory.Instance,
			ErrorFilterTestFactory.CreateLocalizer(),
			ErrorFilterTestFactory.CreateHttpContextAccessor());
		var anyFilter = CreateFilter();
		var exception = new FluentValidation.ValidationException(
			[new FluentValidation.Results.ValidationFailure("Name", "Required")]);

		var validationError = validationFilter.OnError(ErrorFilterTestFactory.CreateError(exception));
		var result = anyFilter.OnError(validationError);

		result.Should().BeSameAs(validationError);
		result.Code.Should().Be("VALIDATION_ERROR");
	}

	private static AnyErrorFilter CreateFilter(ILoggerFactory? loggerFactory = null) =>
		new(
			loggerFactory ?? NullLoggerFactory.Instance,
			ErrorFilterTestFactory.CreateLocalizer(),
			ErrorFilterTestFactory.CreateHttpContextAccessor());

	private sealed class TestNotFoundException(int id) : Exception,
		IStatusCode,
		ILocalizableException,
		IValuedException
	{
		public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
		public string MessageKey => "Domain.NotFound";
		public object[] Arguments => [id];
		public object GetErrorValues() => id;
	}
}
