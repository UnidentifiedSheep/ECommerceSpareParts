using Application.Interfaces;
using Core.Interfaces;
using Core.Interfaces.CacheRepositories;
using MediatR;

namespace Application.Behaviors;

public class CacheBehavior<TRequest, TResponse>(ICache cache, IEnumerable<ICacheableQuery<TRequest>> cacheableList,
    IRelatedDataRepositoryFactory relatedDataRepositoryFactory) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheable = cacheableList.FirstOrDefault();
        if (cacheable == null)
            return await next(cancellationToken);
        
        string cacheKey = cacheable.GetCacheKey(request);
        int duration = cacheable.GetDurationSeconds(request);
        string entityId = cacheable.GetEntityId(request);
        Type relatedType = cacheable.GetRelatedType(request);
        var relatedDataRepository = relatedDataRepositoryFactory.GetRepository(relatedType);
        
        await relatedDataRepository.AddRelatedDataAsync(entityId, cacheKey);
        var cacheValue = await cache.StringGetAsync<TResponse>(cacheKey);
        if (cacheValue != null)
            return cacheValue;
        
        var response = await next(cancellationToken);
        
        await cache.StringSetAsync(cacheKey, response, TimeSpan.FromSeconds(duration));
        return response;
    }
}