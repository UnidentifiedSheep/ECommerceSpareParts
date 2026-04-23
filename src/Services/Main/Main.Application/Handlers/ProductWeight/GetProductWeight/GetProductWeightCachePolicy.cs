using Application.Common.Interfaces;
using Main.Abstractions.Constants;

namespace Main.Application.Handlers.ProductWeight.GetProductWeight;

public class GetProductWeightCachePolicy : ICachePolicy<GetProductWeightQuery>
{
    public string GetCacheKey(GetProductWeightQuery request)
        => string.Format(CacheKeys.ProductWeightCacheKey, request.ProductId);

    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}