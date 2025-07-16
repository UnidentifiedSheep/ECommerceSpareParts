using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles.SetArticlesContentCount;

public record SetArticlesContentCountRequest(int Count);

public class SetArticlesContentCountEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/articles/{articleId}/contents/{insideArticleId}", async (ISender sender, int articleId, 
            int insideArticleId, SetArticlesContentCountRequest request, CancellationToken token) =>
        {
            var command = new SetArticlesContentCountCommand(articleId, insideArticleId, request.Count);
            await sender.Send(command, token);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
        .WithGroup("Articles")
        .WithName("Установка входящего количества в содержимое артикула");
    }
}