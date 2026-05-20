using Abstractions.Models;
using Application.Common.Interfaces.Cqrs;
using Enums;
using Extensions;
using Search.Application.Dtos.Products;
using Search.Application.Interfaces;
using Search.Application.Mapping;

namespace Search.Application.Handlers.Products.SearchProducts;

public record SearchProductsQuery(
    string? Query,
    int? ProducerId,
    Pagination Pagination,
    RangeModel<decimal>? Length = null,
    RangeModel<decimal>? Width = null,
    RangeModel<decimal>? Height = null,
    DimensionUnit DimensionUnit = DimensionUnit.Meter) : IQuery<SearchProductsResult>;

public record SearchProductsResult(IEnumerable<ProductDto> Products);

public class SearchProductsHandler(IProductRepository productRepository)
    : IQueryHandler<SearchProductsQuery, SearchProductsResult>
{
    public async Task<SearchProductsResult> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.Search(
            request.Query ?? string.Empty,
            request.ProducerId,
            request.Pagination,
            ConvertDimensionRangeToMeters(request.Length, request.DimensionUnit),
            ConvertDimensionRangeToMeters(request.Width, request.DimensionUnit),
            ConvertDimensionRangeToMeters(request.Height, request.DimensionUnit),
            cancellationToken);

        return new SearchProductsResult(
            products.Select(x => x.ToProductDto()));
    }

    private static RangeModel<decimal>? ConvertDimensionRangeToMeters(
        RangeModel<decimal>? range,
        DimensionUnit unit)
    {
        return range is null
            ? null
            : new RangeModel<decimal>(
                range.Min?.ToMeters(unit),
                range.Max?.ToMeters(unit));
    }
}
