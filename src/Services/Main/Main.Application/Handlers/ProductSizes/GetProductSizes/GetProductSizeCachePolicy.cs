using Application.Common.Interfaces;
using Main.Abstractions.Constants;

namespace Main.Application.Handlers.ProductSizes.GetProductSizes;

public class GetProductSizeCachePolicy : ICachePolicy<GetProductSizeQuery>
{
    public string GetCacheKey(GetProductSizeQuery request)
        => string.Format(CacheKeys.ProductSizeCacheKey, request.ProductId);

    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}