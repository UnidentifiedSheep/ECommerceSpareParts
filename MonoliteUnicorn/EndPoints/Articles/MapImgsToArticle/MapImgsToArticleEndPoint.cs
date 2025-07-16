using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles.MapImgsToArticle;

public class MapImgsToArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId}/imgs/",
                async (ISender sender, int articleId, HttpContext context, CancellationToken token) =>
                {
                    var files = context.Request.Form.Files;
                    var command = new MapImgsToArticleCommand(files, articleId);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithMetadata()
            .RequireAuthorization("AMW")
            .WithGroup("Articles")
            .WithName("Добавить изображение к артикулу");
    }
}