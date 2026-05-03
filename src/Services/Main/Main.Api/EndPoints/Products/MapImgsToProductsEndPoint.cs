using Api.Common.Extensions;
using Api.Common.Models;
using Carter;
using Main.Application.Handlers.Products.MapImgsToProduct;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public class MapImgsToProductsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId}/imgs/",
                async (ISender sender, int productId, HttpContext context, CancellationToken token) =>
                {
                    var files = FileModel.GetFileModels(context.Request.Form.Files);
                    var command = new MapImgsToProductCommand(productId, files);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithMetadata()
            .WithTags("Articles")
            .WithName("Добавить изображение к артикулу")
            .Produces(200)
            .ProducesProblem(404)
            .RequireAnyPermission("ARTICLE.IMAGES.CREATE");
    }
}