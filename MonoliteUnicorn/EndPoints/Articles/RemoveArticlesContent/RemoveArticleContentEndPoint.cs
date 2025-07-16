using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles.RemoveArticlesContent;

public class RemoveArticleContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/{articleId}/contents/{insideArticleId}",
            async (ISender sender, int articleId, int insideArticleId, CancellationToken token) =>
            {
                var command = new RemoveArticleContentCommand(articleId, insideArticleId);
                await sender.Send(command, token);
                return Results.NoContent();
            }).RequireAuthorization("AMW")
            .WithGroup("Articles")
            .WithDescription("Удаление содержимого артикула в бд")
            .WithDisplayName("Удаление содержимого артикула");
    }
}