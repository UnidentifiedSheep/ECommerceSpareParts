using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Entities.Product;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public class GetProductCrossesCache(
    ICacheKey<GetProductCrossesResult> cacheKey) 
    : ICachePolicy<GetProductCrossesQuery>
{
    public string GetCacheKey(GetProductCrossesQuery request) 
        => cacheKey.FormatKey(request.ArticleId, request.Pagination.Page,
            request.Pagination.Size, request.SortBy);
    
    public int DurationSeconds => 600;
    public Type RelatedType => typeof(ProductCross);
}

public sealed class GetProductCrossesCacheKey : CacheKeyBase<GetProductCrossesResult>
{
    public override string KeyTemplate => "product-crosses:{0}-{1}-{2}-{3}";
}