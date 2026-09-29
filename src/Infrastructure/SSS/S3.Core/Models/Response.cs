using System.Net;
using S3.Core.Interfaces;

namespace S3.Core.Models;

public sealed class Response<T> : IDisposable where T : IResponse
{
	private Response(T? value, HttpStatusCode statusCode, string? errorCode)
	{
		Value = value;
		StatusCode = statusCode;
		ErrorCode = errorCode;
	}

	public T? Value { get; }

	public HttpStatusCode StatusCode { get; }

	public string? ErrorCode { get; }

	public bool IsSuccess => (int)StatusCode is >= 200 and < 300;

	public static Response<T> Success(T value) => new(value, HttpStatusCode.OK, null);

	public static Response<T> Failure(HttpStatusCode statusCode, string? errorCode = null) =>
		new(default, statusCode, errorCode);

	public void Dispose() => Value?.Dispose();
}
