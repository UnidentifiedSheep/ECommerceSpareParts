using Application.Handlers.Articles;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles;

public class MapImgsToArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId}/imgs/",
                async (ISender sender, int articleId, HttpContext context, CancellationToken token) =>
                {
                    //TODO: MAKE THIS
                    var files = context.Request.Form.Files;
                    var command = new MapImgsToArticleCommand();
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithMetadata()
            .RequireAuthorization("AMW")
            .WithTags("Articles")
            .WithName("Добавить изображение к артикулу");
    }
}