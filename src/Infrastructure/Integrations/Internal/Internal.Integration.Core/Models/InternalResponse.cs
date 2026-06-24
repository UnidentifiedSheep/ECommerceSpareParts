using System.Net;

namespace Internal.Integration.Core.Models;

public record InternalResponse<T>
{
    public required bool Success { get; init; }
    public T? Value { get; init; }
    public T ValueOrThrow => Value ?? throw new InvalidOperationException("Value is null");
    public HttpStatusCode? StatusCode { get; init; }
    public string? Error { get; init; }

    public static InternalResponse<T> Ok(T value) => new()
    {
        Success = true,
        Value = value
    };

    public static InternalResponse<T> Fail(HttpStatusCode statusCode, string? error = null) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Error = error
    };
}