using Application.Common.Interfaces;
using Core.Interfaces;
using Core.Interfaces.CacheRepositories;
using MediatR;

namespace Application.Common.Behaviors;

public class CacheBehavior<TRequest, TResponse>(
    ICache cache,
    IRelatedDataFactory relatedDataFactory)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next(cancellationToken);

        var cacheKey = cacheable.GetCacheKey();

        var cacheValue = await cache.StringGetAsync<TResponse>(cacheKey);
        if (cacheValue != null)
            return cacheValue;

        var response = await next(cancellationToken);

        var duration = cacheable.GetDurationSeconds();
        var relatedType = cacheable.GetRelatedType();
        var relatedDataRepository = relatedDataFactory.GetRepository(relatedType);
        await relatedDataRepository.AddRelatedDataAsync(cacheable.RelatedEntityIds, cacheKey);

        await cache.StringSetAsync(cacheKey, response, TimeSpan.FromSeconds(duration));
        return response;
    }

}