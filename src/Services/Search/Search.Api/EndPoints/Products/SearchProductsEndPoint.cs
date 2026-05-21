using System.Text.Json.Serialization;
using Abstractions.Models;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Search.Application.Dtos.Products;
using Search.Application.Handlers.Products.SearchProducts;

namespace Search.Api.EndPoints.Products;

public record SearchProductsRequest : PaginationQueryModel
{
    [FromQuery(Name = "query")]
    public string? Query { get; init; }
    
    [FromQuery(Name = "producerId")]
    public int? ProducerId { get; init; }

    [FromQuery(Name = "lengthMin")]
    public decimal? LengthMin { get; init; }

    [FromQuery(Name = "lengthMax")]
    public decimal? LengthMax { get; init; }

    [FromQuery(Name = "widthMin")]
    public decimal? WidthMin { get; init; }

    [FromQuery(Name = "widthMax")]
    public decimal? WidthMax { get; init; }

    [FromQuery(Name = "heightMin")]
    public decimal? HeightMin { get; init; }

    [FromQuery(Name = "heightMax")]
    public decimal? HeightMax { get; init; }

    [FromQuery(Name = "dimensionUnit")]
    public DimensionUnit? DimensionUnit { get; init; }
}

public record SearchProductsResult
{
    [JsonPropertyName("products")]
    public required IEnumerable<ProductDto> Products { get; init; }
}


public class SearchProductsEndPoint : ICarterModule
{
    private const int MaxLimit = 100;

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (
                ISender sender,
                [AsParameters] SearchProductsRequest request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new SearchProductsQuery(
                        request.Query,
                        request.ProducerId,
                        request,
                        ToRange(request.LengthMin, request.LengthMax),
                        ToRange(request.WidthMin, request.WidthMax),
                        ToRange(request.HeightMin, request.HeightMax),
                        request.DimensionUnit ?? DimensionUnit.Meter),
                    cancellationToken);

                return Results.Ok(new SearchProductsResult
                {
                    Products = result.Products
                });
            })
            .WithTags("Products")
            .WithDisplayName("Search products")
            .Produces<SearchProductsResult>();
    }

    private static RangeModel<decimal>? ToRange(decimal? min, decimal? max)
    {
        return min.HasValue || max.HasValue
            ? new RangeModel<decimal>(min, max)
            : null;
    }
}
