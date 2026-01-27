namespace Application.Common.Interfaces;

public interface ICacheableQuery
{
    string GetCacheKey();
    /// <summary>
    /// Entity type used for cache dependency invalidation. null = no related tracking.
    /// </summary>
    /// <returns></returns>
    Type? GetRelatedType();
    int GetDurationSeconds();
}