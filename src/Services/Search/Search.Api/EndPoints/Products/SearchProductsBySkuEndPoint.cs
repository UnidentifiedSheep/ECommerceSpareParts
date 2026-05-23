using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Search.Application.Dtos.Products;
using Search.Application.Handlers.Products.SearchProductsBySku;

namespace Search.Api.EndPoints.Products;

public record SearchProductsBySkuRequest : PaginationQueryModel
{
    [FromQuery(Name = "sku")]
    public required string Sku { get; init; }
    
    [FromQuery(Name = "producerId")]
    public int? ProducerId { get; init; }
}

public record SearchProductsBySkuResult
{
    [JsonPropertyName("products")]
    public required IEnumerable<ProductDto> Products { get; init; }
}

public static class SearchProductsBySkuEndPoint
{
    public static RouteGroupBuilder SearchProductsBySku(this RouteGroupBuilder products)
    {
        products.MapGet("/sku", async (
                ISender sender,
                [AsParameters] SearchProductsBySkuRequest request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new SearchProductsBySkuQuery(
                        request.Sku,
                        request.ProducerId,
                        request),
                    cancellationToken);

                return Results.Ok(new SearchProductsBySkuResult
                {
                    Products = result.Products
                });
            })
            .WithTags("Products")
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithDisplayName("Search products")
            .Produces<SearchProductsBySkuResult>();
        
        return products;
    }
}