using Abstractions.Interfaces.Cache;
using Application.Common.Abstractions;
using Application.Common.Interfaces;

namespace Main.Application.Handlers.ProductSizes.GetProductSizes;

public class GetProductSizeCachePolicy(ICacheKeyRegistry keyRegistry) : ICachePolicy<GetProductSizeQuery>
{
    public string GetCacheKey(GetProductSizeQuery request) 
        => keyRegistry.FormatKey<GetProductSizesResult, int>(request.ProductId);
    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}
public sealed class GetProductSizesCacheInvalidator(
    ICache cache, 
    ICacheKeyRegistry keyRegistry) 
    : SingleEntityCacheInvalidatorBase<GetProductSizesResult, int>(cache, keyRegistry)
{
}