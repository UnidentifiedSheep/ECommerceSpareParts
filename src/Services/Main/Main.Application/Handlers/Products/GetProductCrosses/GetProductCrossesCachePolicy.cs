using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Entities.Product;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public class GetProductCrossesCachePolicy(
    ICacheKeyRegistry keyRegistry) 
    : ICachePolicy<GetProductCrossesQuery>
{
    public string GetCacheKey(GetProductCrossesQuery request) 
        => keyRegistry.FormatKey<GetProductCrossesResult, (int id, Pagination pagination, string? sortBy)>
            ((request.ProductId, request.Pagination, request.SortBy));
    
    public int DurationSeconds => 600;
    public Type RelatedType => typeof(ProductCross);
}