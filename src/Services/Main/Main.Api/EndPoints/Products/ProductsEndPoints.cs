using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Application.Handlers.Products.PatchProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Products;

public record CreateProductRequest(List<CreateProductDto> NewProducts);

public record CreateProductResponse(IReadOnlyList<int> CreatedIds);

public record EditProductRequest(PatchProductDto PatchProduct);

public record GetProductByIdsRequest
{
    [FromQuery(Name = "id")]
    public int[] Ids { get; init; } = [];
}

public record GetProductByIdsResult
{
    [JsonPropertyName("products")]
    public required IReadOnlyList<ProductDto> Products { get; init; }
}

public record GetProductByIdResponse
{
    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }
}

public record GetProductStockResponse
{
    [JsonPropertyName("stock")]
    public required int Stock { get; init; }
}

public class ProductsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products")
            .WithTags("Product");

        products.MapProductContentEndPoints();
        products.MapProductRelationsEndPoints();
        products.MapProductImagesEndPoints();
        products.MapProductReservationsEndPoints();

        products.MapPost(
                "",
                async (
                    ISender sender,
                    CreateProductRequest request,
                    CancellationToken token) =>
                {
                    var command = new CreateProductsCommand(request.NewProducts);
                    var result = await sender.Send(command, token);
                    var response = new CreateProductResponse(result.CreatedIds);
                    return Results.Created("/products", response);
                })
            .WithName("CreateProducts")
            .WithSummary("Создать продукты")
            .WithDescription("Добавление новых артикулов")
            .WithDisplayName("Добавление артикулов")
            .Accepts<CreateProductRequest>(false, "application/json")
            .Produces<CreateProductResponse>(201, "application/json")
            .ProducesProblem(400)
            .RequireAnyPermission(PermissionCodes.ARTICLES_CREATE);

        products.MapGet(
                "",
                async (
                    ISender sender,
                    [AsParameters] GetProductByIdsRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetProductByIdsQuery(request.Ids), token);
                    return Results.Ok(
                        new GetProductByIdsResult
                        {
                            Products = result.Products
                        });
                })
            .WithName("GetProductsByIds")
            .WithSummary("Получить продукты по идентификаторам")
            .WithDescription("Получение списка артикулов по идентификаторам")
            .WithDisplayName("Получение артикулов по идентификаторам")
            .Produces<GetProductByIdsResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ARTICLES_GET_MAIN);

        products.MapGet(
                "/{productId:int}",
                async (
                    ISender sender,
                    int productId,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetByIdQuery(productId), token);
                    return Results.Ok(
                        new GetProductByIdResponse
                        {
                            Product = result.Product
                        });
                })
            .WithName("GetProductById")
            .WithSummary("Получить продукт по идентификатору")
            .WithDescription("Получение артикула по идентификатору")
            .WithDisplayName("Получение артикула по идентификатору")
            .Produces<GetProductByIdResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLES_GET_MAIN);

        products.MapGet(
                "/{productId:int}/stock",
                async (
                    ISender sender,
                    int productId,
                    [FromQuery] string? storageName,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new GetProductStockQuery(productId, storageName),
                        token);

                    return Results.Ok(
                        new GetProductStockResponse
                        {
                            Stock = result.Stock
                        });
                })
            .WithName("GetProductStock")
            .WithSummary("Получить остаток продукта")
            .WithDescription("Получение общего остатка артикула или остатка на конкретном складе")
            .WithDisplayName("Получение остатка артикула")
            .Produces<GetProductStockResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ARTICLES_GET_MAIN);

        products.MapPatch(
                "/{productId:int}",
                async (
                    ISender sender,
                    int productId,
                    EditProductRequest request,
                    CancellationToken token) =>
                {
                    var command = new PatchProductCommand(productId, request.PatchProduct);
                    await sender.Send(command, token);
                    return Results.NoContent();
                })
            .WithName("EditProduct")
            .WithSummary("Редактировать продукт")
            .WithDescription("Редактирование артикула")
            .WithDisplayName("Редактирование артикула")
            .Accepts<EditProductRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLES_EDIT);
    }
}
