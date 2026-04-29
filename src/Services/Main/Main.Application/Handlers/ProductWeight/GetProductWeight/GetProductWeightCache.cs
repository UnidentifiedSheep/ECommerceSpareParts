using Abstractions.Interfaces.Cache;
using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Abstractions.Constants;

namespace Main.Application.Handlers.ProductWeight.GetProductWeight;

public class GetProductWeightCache(ICacheKeyRegistry keyRegistry) : ICachePolicy<GetProductWeightQuery>
{
    public string GetCacheKey(GetProductWeightQuery request)
        => keyRegistry.FormatKey<GetProductWeightResult, int>(request.ProductId);

    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}

public sealed class GetProductWeightCacheInvalidator(
    ICacheKeyRegistry keyRegistry,
    ICache cache) : SingleEntityCacheInvalidatorBase<GetProductWeightResult, int>(cache, keyRegistry)
{
}