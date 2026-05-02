using Application.Common.Interfaces;
using MediatR;
using ZiggyCreatures.Caching.Fusion;

namespace Application.Common.Behaviors;

public class CacheBehavior<TRequest, TResponse>(
    IFusionCache cache,
    IIdsCollector idsCollector,
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

        List<string>? tags = null;
        var cacheKey = cachePolicy.GetCacheKey(request);

        if (cachePolicy.Tags is { Count: > 0 })
        {
            tags = [];
            foreach (var id in idsCollector.CurrentIds)
                tags.AddRange(cachePolicy.Tags.Select(tag => $"{tag}:{id}"));
        }

        return await cache.GetOrSetAsync<TResponse>(
            key: cacheKey,
            factory: ct => next(ct),
            duration: cachePolicy.TimeToLive,
            tags: tags,
            token: cancellationToken);
    }
}