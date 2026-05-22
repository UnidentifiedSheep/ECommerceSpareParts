using Api.Common.Extensions;
using Api.Common.Models;
using Enums;
using Main.Application.Handlers.Products.MapImgsToProduct;
using MediatR;

namespace Main.Api.EndPoints.Products;

public static class ProductImagesEndPoints
{
    public static RouteGroupBuilder MapProductImagesEndPoints(this RouteGroupBuilder products)
    {
        products.MapPost("/{productId}/imgs/", async (
                ISender sender,
                int productId,
                HttpContext context,
                CancellationToken token) =>
            {
                var files = FileModel.GetFileModels(context.Request.Form.Files);
                var command = new MapImgsToProductCommand(productId, files);
                await sender.Send(command, token);
                return Results.Ok();
            })
            .WithMetadata()
            .WithName("Добавить изображение к артикулу")
            .Produces(200)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.ARTICLE_IMAGES_CREATE);

        return products;
    }
}
