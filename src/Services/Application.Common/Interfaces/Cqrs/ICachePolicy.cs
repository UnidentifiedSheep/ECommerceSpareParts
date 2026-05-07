namespace Application.Common.Interfaces;

public interface ICachePolicy<in TRequest>
{
    string GetCacheKey(TRequest request);
    TimeSpan TimeToLive { get; }
    IReadOnlyCollection<string>? Tags { get; }
    string? BaseTag { get; }
    
}