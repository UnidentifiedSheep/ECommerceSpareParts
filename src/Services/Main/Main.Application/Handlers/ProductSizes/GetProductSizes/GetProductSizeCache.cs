using Abstractions.Interfaces.Cache;
using Application.Common.Abstractions;
using Application.Common.Interfaces;

namespace Main.Application.Handlers.ProductSizes.GetProductSizes;

public class GetProductSizeCache(ICacheKey<GetProductSizesResult> cacheKey) : ICachePolicy<GetProductSizeQuery>
{
    public string GetCacheKey(GetProductSizeQuery request) => cacheKey.FormatKey(request.ProductId);
    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}

public sealed class GetProductSizeCacheKey : CacheKeyBase<GetProductSizesResult>
{
    public override string KeyTemplate => "product-size:{0}";
}

public sealed class GetProductSizesCacheInvalidator(
    ICache cache, 
    ICacheKey<GetProductSizesResult> cacheKey) 
    : SingleEntityCacheInvalidatorBase<GetProductSizesResult, int>(cache, cacheKey)
{
}