using System.Text.Json.Serialization;
using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Search.Application.Dtos.Products;
using Search.Application.Handlers.Products.SearchProductsByAll;

namespace Search.Api.EndPoints.Products;

public record SearchProductsByAllRequest : PaginationQueryModel
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

public record SearchProductsByAllResult
{
    [JsonPropertyName("products")]
    public required IEnumerable<ProductDto> Products { get; init; }
}


public static class SearchProductsByAllEndPoint
{
    public static RouteGroupBuilder AddSearchProductsByAll(this RouteGroupBuilder products)
    {
        products.MapGet("/all", async (
                ISender sender,
                [AsParameters] SearchProductsByAllRequest request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new SearchProductsByAllQuery(
                        request.Query,
                        request.ProducerId,
                        request,
                        ToRange(request.LengthMin, request.LengthMax),
                        ToRange(request.WidthMin, request.WidthMax),
                        ToRange(request.HeightMin, request.HeightMax),
                        request.DimensionUnit ?? DimensionUnit.Meter),
                    cancellationToken);

                return Results.Ok(new SearchProductsByAllResult
                {
                    Products = result.Products
                });
            })
            .WithTags("Products")
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithDisplayName("Search products")
            .Produces<SearchProductsByAllResult>();
        
        return products;
    }

    private static RangeModel<decimal>? ToRange(decimal? min, decimal? max)
    {
        return min.HasValue || max.HasValue
            ? new RangeModel<decimal>(min, max)
            : null;
    }
}
