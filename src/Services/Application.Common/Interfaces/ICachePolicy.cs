namespace Application.Common.Interfaces;

public interface ICachePolicy<in TRequest>
{
    string GetCacheKey(TRequest request);
    int DurationSeconds { get; }
    /// <summary>
    /// Entity type used for cache dependency invalidation. null = no related tracking.
    /// </summary>
    Type? RelatedType { get; }
}