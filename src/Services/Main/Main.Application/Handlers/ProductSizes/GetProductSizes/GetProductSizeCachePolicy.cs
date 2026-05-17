using Application.Common.Interfaces.Cqrs;

namespace Main.Application.Handlers.ProductSizes.GetProductSizes;

public class GetProductSizeCachePolicy : ICachePolicy<GetProductSizeQuery>
{
    public string GetCacheKey(GetProductSizeQuery request)
    {
        return CacheKeys.ProductCache.ProductSizes(request.ProductId);
    }

    public TimeSpan TimeToLive => CacheKeys.ProductCache.Ttl;
    public IReadOnlyCollection<string> Tags => ["product"];
    public string? BaseTag => null;
}