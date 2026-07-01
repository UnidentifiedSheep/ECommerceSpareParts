using Api.Common.Extensions;
using Api.Common.Models;
using Enums;
using Main.Application.Handlers.Products;
using Main.Application.Handlers.Products.MapImgsToProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Products;

public static class ProductImagesEndPoints
{
    public static RouteGroupBuilder MapProductImagesEndPoints(this RouteGroupBuilder products)
    {
        products.MapPost(
                "/{productId:int}/imgs/",
                async (
                    ISender sender,
                    int productId,
                    IFormFileCollection files,
                    CancellationToken token) =>
                {
                    var command = new MapImgsToProductCommand(productId, FileModel.GetFileModels(files));
                    await sender.Send(command, token);
                    return Results.Ok();
                })
            .DisableAntiforgery()
            .WithMetadata()
            .WithName("AddProductImages")
            .WithSummary("Добавить изображения продукта")
            .WithDescription("Добавление изображений продукта")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.ARTICLE_IMAGES_CREATE);

        products.MapDelete(
                "/{productId:int}/imgs",
                async (
                    ISender sender,
                    int productId,
                    [FromQuery] string imagePath,
                    CancellationToken token) =>
                {
                    var command = new RemoveProductImageCommand(productId, imagePath);
                    await sender.Send(command, token);
                    return Results.NoContent();
                })
            .DisableAntiforgery()
            .WithMetadata()
            .WithName("DeleteProductImage")
            .WithSummary("Удаление изображения продукта")
            .WithDescription("Удаление изображений продукта")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.ARTICLE_IMAGES_DELETE);

        return products;
    }
}