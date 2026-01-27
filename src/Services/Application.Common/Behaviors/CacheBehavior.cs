using Application.Common.Interfaces;
using Core.Interfaces;
using Core.Interfaces.CacheRepositories;
using MediatR;

namespace Application.Common.Behaviors;

public class CacheBehavior<TRequest, TResponse>(ICache cache, IRelatedDataFactory relatedDataFactory, 
    IRelatedDataCollector relatedDataCollector) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next(cancellationToken);

        var cacheKey = cacheable.GetCacheKey();

        var cached = await cache.StringGetAsync<TResponse>(cacheKey);
        if (cached != null)
            return cached;

        var duration = cacheable.GetDurationSeconds();
        var relatedType = cacheable.GetRelatedType();

        using var _ = relatedDataCollector.BeginScope();

        var response = await next(cancellationToken);

        if (relatedType != null)
        {
            var relatedRepo = relatedDataFactory.GetRepository(relatedType);
            var ids = relatedDataCollector.CurrentIds;

            if (ids.Count > 0)
                await relatedRepo.AddRelatedDataAsync(ids, cacheKey);
        }

        await cache.StringSetAsync(cacheKey, response, TimeSpan.FromSeconds(duration));

        return response;
    }
}
