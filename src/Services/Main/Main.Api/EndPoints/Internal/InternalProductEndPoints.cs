using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products;
using MediatR;

namespace Main.Api.EndPoints.Internal;

public record InternalGetFullProductResponse
{
    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }

    [JsonPropertyName("productWeight")]
    public ProductWeightDto? ProductWeight { get; init; }

    [JsonPropertyName("productSize")]
    public ProductSizeDto? ProductSize { get; init; }
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
                "{id:int}/full",
                async (
                    ISender sender,
                    int id,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetFullProductQuery(id),
                        cancellationToken);

                    return Results.Ok(
                        new InternalGetFullProductResponse
                        {
                            Product = result.Product,
                            ProductWeight = result.ProductWeight,
                            ProductSize = result.ProductSize
                        });
                })
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithGroupName("Internal Products")
            .WithDisplayName("Internal service full product")
            .WithName("InternalFullProduct")
            .WithSummary("Получить полный продукт для внутреннего сервиса")
            .WithDescription("Получение продукта, веса и размеров для внутренних интеграций")
            .Produces<InternalGetFullProductResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}