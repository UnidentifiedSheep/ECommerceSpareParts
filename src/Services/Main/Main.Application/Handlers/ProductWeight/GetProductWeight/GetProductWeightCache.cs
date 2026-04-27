using Abstractions.Interfaces.Cache;
using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Abstractions.Constants;

namespace Main.Application.Handlers.ProductWeight.GetProductWeight;

public class GetProductWeightCache(ICacheKey<GetProductWeightResult> cacheKey) : ICachePolicy<GetProductWeightQuery>
{
    public string GetCacheKey(GetProductWeightQuery request)
        => cacheKey.FormatKey(request.ProductId);

    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}

public class GetProductWeightCacheKey : CacheKeyBase<GetProductWeightResult>
{
    public override string KeyTemplate => "product-weight:{0}";
}

public sealed class GetProductWeightCacheInvalidator(
    ICacheKey<GetProductWeightResult> cacheKey,
    ICache cache) : SingleEntityCacheInvalidatorBase<GetProductWeightResult, int>(cache, cacheKey)
{
}