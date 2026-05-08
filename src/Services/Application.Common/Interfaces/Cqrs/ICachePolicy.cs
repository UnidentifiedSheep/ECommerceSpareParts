namespace Application.Common.Interfaces;

public interface ICachePolicy<in TRequest>
{
    TimeSpan TimeToLive { get; }
    IReadOnlyCollection<string>? Tags { get; }
    string? BaseTag { get; }
    string GetCacheKey(TRequest request);
}