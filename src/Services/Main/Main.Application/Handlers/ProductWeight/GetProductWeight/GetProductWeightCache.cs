using Application.Common.Interfaces.Cqrs;
using Main.Application.Static;

namespace Main.Application.Handlers.ProductWeight.GetProductWeight;

public class GetProductWeightCache : ICachePolicy<GetProductWeightQuery>
{
    public string GetCacheKey(GetProductWeightQuery request)
    {
        return CacheKeys.ProductCache.ProductWeight(request.ProductId);
    }

    public TimeSpan TimeToLive => CacheKeys.ProductCache.Ttl;
    public IReadOnlyCollection<string> Tags => ["product"];
    public string? BaseTag => null;
}