namespace Application.Interfaces;

public interface ICacheableQuery
{
    HashSet<string> RelatedEntityIds { get; }
    string GetCacheKey();
    Type GetRelatedType();
    int GetDurationSeconds();
}