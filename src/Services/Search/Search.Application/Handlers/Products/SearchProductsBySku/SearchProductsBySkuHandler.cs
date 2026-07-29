using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Extensions;
using Application.Common.Interfaces.Projections;
using Search.Application.Dtos.Products;
using Search.Application.Interfaces.Product;
using Search.Entities;
using Search.Enums;

namespace Search.Application.Handlers.Products.SearchProductsBySku;

public record SearchProductsBySkuQuery(
    string Sku,
    int? ProducerId,
    SkuSearchMode SearchMode,
    Pagination Pagination,
    string[] SortBy
) : IQuery<SearchProductsBySkuResult>;

public record SearchProductsBySkuResult(IEnumerable<ProductDto> Products);

public class SearchProductsBySkuHandler(
    IProductRepository productRepository,
    IProjectionProvider<Product, ProductDto> projection
) : IQueryHandler<SearchProductsBySkuQuery, SearchProductsBySkuResult>
{
    public async Task<SearchProductsBySkuResult> Handle(
        SearchProductsBySkuQuery request,
        CancellationToken cancellationToken)
    {
        var result = await productRepository.SearchBySku(
            request.Sku,
            request.ProducerId,
            request.SearchMode,
            request.Pagination,
            request.SortBy,
            cancellationToken);

        return new SearchProductsBySkuResult(
            result.Select(projection.Projection.AsFunc()));
    }
}
