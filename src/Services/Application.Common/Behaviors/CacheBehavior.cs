using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
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

        var cacheKey = cachePolicy.GetCacheKey(request);
        var cached = await cache.TryGetAsync<TResponse>(cacheKey, token: cancellationToken);
        if (cached.HasValue)
            return cached.Value;

        using var _ = idsCollector.BeginScope();
        var response = await next(cancellationToken);

        List<string>? tags = null;
        if (cachePolicy.Tags is { Count: > 0 })
        {
            tags = [];
            if (cachePolicy.BaseTag != null)
                tags.Add(cachePolicy.BaseTag);
            foreach (var id in idsCollector.CurrentIds)
                tags.AddRange(cachePolicy.Tags.Select(tag => $"{tag}:{id}"));
        }

        await cache.SetAsync(
            cacheKey,
            response,
            cachePolicy.TimeToLive,
            tags,
            cancellationToken);

        return response;
    }
}
