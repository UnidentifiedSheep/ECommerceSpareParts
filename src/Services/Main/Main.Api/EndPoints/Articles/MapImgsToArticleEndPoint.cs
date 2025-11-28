using Api.Common.Extensions;
using Api.Common.Models;
using Carter;
using Main.Application.Handlers.ArticleImages.MapImgsToArticle;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public class MapImgsToArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId}/imgs/",
                async (ISender sender, int articleId, HttpContext context, CancellationToken token) =>
                {
                    var files = FileModel.GetFileModels(context.Request.Form.Files);
                    var command = new MapImgsToArticleCommand(articleId, files);
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