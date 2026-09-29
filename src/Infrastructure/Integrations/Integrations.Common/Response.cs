using System.Net;

namespace Integrations.Common;

public record Response<T>
{
	public required bool Success { get; init; }

	public T? Value { get; init; }

	public T ValueOrThrow => Value ?? throw new InvalidOperationException("Value is null");

	public HttpStatusCode StatusCode { get; init; }

	public string? Error { get; init; }

	public static Response<T> Ok(T? value, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		return new Response<T>
		{
			Success = true, Value = value, StatusCode = statusCode
		};
	}

	public static Response<T> Fail(HttpStatusCode statusCode, string? error = null)
	{
		return new Response<T>
		{
			Success = false,
			StatusCode = statusCode,
			Error = error
		};
	}

	public static Response<T> FromFail<TOther>(Response<TOther> fail) => Fail(fail.StatusCode, fail.Error);
}
