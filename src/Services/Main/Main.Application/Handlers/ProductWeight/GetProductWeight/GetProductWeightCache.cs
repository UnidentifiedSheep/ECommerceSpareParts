using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;

namespace Main.Application.Handlers.ProductWeight.GetProductWeight;

public class GetProductWeightCache : ICachePolicy<GetProductWeightQuery>
{
    public string GetCacheKey(GetProductWeightQuery request) => CacheKeys.ProductCache.ProductWeight(request.ProductId);
    public TimeSpan TimeToLive => CacheKeys.ProductCache.Ttl;
    public IReadOnlyCollection<string> Tags => ["product"];
    public string? BaseTag => null;
}