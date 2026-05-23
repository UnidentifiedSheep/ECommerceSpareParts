using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Search.Application.Dtos.Products;
using Search.Application.Interfaces;
using Search.Application.Mapping;

namespace Search.Application.Handlers.Products.SearchProductsBySku;

public record SearchProductsBySkuQuery(
    string Sku,
    int? ProducerId,
    Pagination Pagination) : IQuery<SearchProductsBySkuResult>;
public record SearchProductsBySkuResult(IEnumerable<ProductDto> Products);

public class SearchProductsBySkuHandler(
    IProductRepository productRepository
    ) : IQueryHandler<SearchProductsBySkuQuery, SearchProductsBySkuResult>
{
    public async Task<SearchProductsBySkuResult> Handle(SearchProductsBySkuQuery request, CancellationToken cancellationToken)
    {
        var result = await productRepository.SearchBySku(
            request.Sku,
            request.ProducerId,
            request.Pagination,
            cancellationToken);
        
        return new SearchProductsBySkuResult(
            result.Select(x => x.ToProductDto()));
    }
}