using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products;
using Main.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Internal;

public record InternalGetFullProductsResponse
{
    [JsonPropertyName("products")]
    public required IReadOnlyList<FullProductDto> Products { get; init; }
}

public record InternalGetFullProductsRequest
{
    [FromQuery(Name = "id")]
    public int[] ProductIds { get; init; } = [];
}

public record InternalGetSupplierProductReferencesResponse
{
    [JsonPropertyName("products")]
    public required IReadOnlyList<SupplierProductReferenceDto> Products { get; init; }
}

public record InternalGetSupplierProductReferencesRequest
{
    [FromQuery(Name = "id")]
    public int[] ProductIds { get; init; } = [];

    [FromQuery(Name = "supplier")]
    public Supplier Supplier { get; init; }
}

public static class InternalProductEndPoints
{
    public static RouteGroupBuilder AddInternalProductsEndPoints(this RouteGroupBuilder group)
    {
        var products = group
            .MapGroup("/products")
            .WithGroupName("Internal Products")
            .WithTags("InternalProducts");

        products.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] InternalGetFullProductsRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetFullProductsQuery(request.ProductIds),
                        cancellationToken);

                    return Results.Ok(
                        new InternalGetFullProductsResponse
                        {
                            Products = result.Products
                        });
                })
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithGroupName("Internal Products")
            .WithDisplayName("Internal service full products")
            .WithName("InternalFullProducts")
            .WithSummary("Получить полный продукт для внутреннего сервиса")
            .WithDescription("Получение продукта, веса и размеров для внутренних интеграций")
            .Accepts<InternalGetFullProductsRequest>(true, "application/json")
            .Produces<InternalGetFullProductsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        products.MapGet(
                "supplier-references",
                async (
                    ISender sender,
                    [AsParameters] InternalGetSupplierProductReferencesRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetSupplierProductReferencesQuery(
                            request.ProductIds,
                            request.Supplier),
                        cancellationToken);

                    return Results.Ok(
                        new InternalGetSupplierProductReferencesResponse
                        {
                            Products = result.Products
                        });
                })
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithGroupName("Internal Products")
            .WithDisplayName("Internal service supplier product references")
            .WithName("InternalSupplierProductReferences")
            .WithSummary("Получить данные продуктов для запроса к поставщику")
            .WithDescription("Получение артикула и названия производителя в терминах поставщика для внутренних интеграций")
            .Produces<InternalGetSupplierProductReferencesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
