using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

public class CacheBehavior<TRequest, TResponse>(
    ICache cache,
    IRelatedDataFactory relatedDataFactory,
    IRelatedDataCollector relatedDataCollector,
    ICachePolicy<TRequest>? cachePolicy = null) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (cachePolicy == null)
            return await next(cancellationToken);

        var cacheKey = cachePolicy.GetCacheKey(request);

        var cached = await cache.StringGetAsync<TResponse>(cacheKey);
        if (cached != null)
            return cached;

        var relatedType = cachePolicy.RelatedType;

        using var _ = relatedDataCollector.BeginScope();

        var response = await next(cancellationToken);

        if (relatedType != null)
        {
            var relatedRepo = relatedDataFactory.GetRepository(relatedType);
            var ids = relatedDataCollector.CurrentIds;

            if (ids.Count > 0)
                await relatedRepo.AddRelatedDataAsync(ids, cacheKey);
        }

        await cache.StringSetAsync(cacheKey, response, TimeSpan.FromSeconds(cachePolicy.DurationSeconds));

        return response;
    }
}