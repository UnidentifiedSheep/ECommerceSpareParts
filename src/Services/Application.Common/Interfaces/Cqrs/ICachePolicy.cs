namespace Application.Common.Interfaces.Cqrs;

public interface ICachePolicy<in TRequest>
{
    TimeSpan TimeToLive { get; }
    IReadOnlyCollection<string>? Tags { get; }
    string? BaseTag { get; }
    string GetCacheKey(TRequest request);
}