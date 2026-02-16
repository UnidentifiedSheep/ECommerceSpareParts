namespace Abstractions.Models;

/// <summary>
/// Represents a value with the timestamp.
/// Value can be null.
/// </summary>
/// <typeparam name="T"></typeparam>
public record Timestamped<T>
{
    public T? Value { get; init; }

    public DateTime Timestamp { get; init; }
}