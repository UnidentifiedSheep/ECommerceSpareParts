namespace Application.Interfaces;

public interface ICacheableQuery<in TRequest>
{
    string GetCacheKey(TRequest request);
    string GetEntityId(TRequest request);
    Type GetRelatedType(TRequest request);
    int GetDurationSeconds(TRequest request);
}