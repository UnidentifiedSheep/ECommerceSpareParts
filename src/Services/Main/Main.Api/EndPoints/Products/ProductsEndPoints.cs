using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Application.Handlers.Products.PatchProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApplicationGetProductByIdsQuery = Main.Application.Handlers.Products.GetByIds.GetProductByIdsQuery;

namespace Main.Api.EndPoints.Products;

public record CreateProductRequest(List<CreateProductDto> NewProducts);

public record CreateProductResponse(IReadOnlyList<int> CreatedIds);

public record EditProductRequest(PatchProductDto PatchProduct);

public record GetProductByIdsQuery
{
    [FromQuery(Name = "id")]
    public int[] Ids { get; init; } = [];
}

public record GetProductByIdsResult
{
    [JsonPropertyName("products")]
    public required IReadOnlyList<ProductDto> Products { get; init; }
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

        products.MapPost("", async (
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

        products.MapGet("", async (
                ISender sender,
                [AsParameters] GetProductByIdsQuery request,
                CancellationToken token) =>
            {
                var result = await sender.Send(new ApplicationGetProductByIdsQuery(request.Ids), token);
                return Results.Ok(new GetProductByIdsResult
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

        products.MapPatch("/{productId:int}", async (
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
