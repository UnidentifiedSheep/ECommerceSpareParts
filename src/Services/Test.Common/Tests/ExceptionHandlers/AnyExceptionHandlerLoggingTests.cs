using System.Net;
using Abstractions.Interfaces.Exceptions;
using Api.Common.ExceptionHandlers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tests.Stubs;

namespace Tests.Tests.ExceptionHandlers;

public class AnyExceptionHandlerLoggingTests
{
	[Fact]
	public async Task TryHandleAsync_ShouldLogClientErrorAsInformation()
	{
		var loggerFactory = new RecordingLoggerFactory();
		var handler = new AnyExceptionHandler(loggerFactory.CreateLogger<AnyExceptionHandler>());

		await handler.TryHandleAsync(
			CreateHttpContext(),
			new TestStatusException(HttpStatusCode.BadRequest),
			CancellationToken.None);

		loggerFactory.LogLevels.Should().ContainSingle().Which.Should().Be(LogLevel.Information);
	}

	[Fact]
	public async Task TryHandleAsync_ShouldLogServerErrorAsError()
	{
		var loggerFactory = new RecordingLoggerFactory();
		var handler = new AnyExceptionHandler(loggerFactory.CreateLogger<AnyExceptionHandler>());

		await handler.TryHandleAsync(
			CreateHttpContext(),
			new InvalidOperationException("failure"),
			CancellationToken.None);

		loggerFactory.LogLevels.Should().ContainSingle().Which.Should().Be(LogLevel.Error);
	}

	private static HttpContext CreateHttpContext()
	{
		return new DefaultHttpContext
		{
			RequestServices = new ServiceCollection().BuildServiceProvider(),
			Response =
			{
				Body = new MemoryStream()
			}
		};
	}

	private sealed class TestStatusException(HttpStatusCode statusCode) : Exception, IStatusCode
	{
		public HttpStatusCode StatusCode { get; } = statusCode;
	}
}
