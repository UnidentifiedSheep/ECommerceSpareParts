using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Enums;
using Enums.Units;
using Extensions;
using Search.Application.Dtos.Products;
using Search.Application.Interfaces.Product;
using Search.Entities;

namespace Search.Application.Handlers.Products.SearchProductsByAll;

public record SearchProductsByAllQuery(
    string? Query,
    int? ProducerId,
    Pagination Pagination,
    string[] SortBy,
    RangeModel<decimal>? Length = null,
    RangeModel<decimal>? Width = null,
    RangeModel<decimal>? Height = null,
    DimensionUnit DimensionUnit = DimensionUnit.Meter
) : IQuery<SearchProductsByAllResult>;

public record SearchProductsByAllResult(IEnumerable<ProductDto> Products);

public class SearchProductsByAllHandler(
    IProductRepository productRepository,
    IProjectionProvider<Product, ProductDto> projection)
    : IQueryHandler<SearchProductsByAllQuery, SearchProductsByAllResult>
{
    public async Task<SearchProductsByAllResult> Handle(
        SearchProductsByAllQuery request,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.Search(
            request.Query ?? string.Empty,
            request.ProducerId,
            request.Pagination,
            request.SortBy,
            ConvertDimensionRangeToMeters(request.Length, request.DimensionUnit),
            ConvertDimensionRangeToMeters(request.Width, request.DimensionUnit),
            ConvertDimensionRangeToMeters(request.Height, request.DimensionUnit),
            cancellationToken);

        return new SearchProductsByAllResult(
            products.Select(projection.ProjectionFunc));
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
