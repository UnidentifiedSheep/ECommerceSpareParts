using Main.Application.Handlers.ArticleContent.SetArticleContentCount;
using Carter;
using MediatR;

namespace Main.Api.EndPoints.Articles;

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
            .WithTags("Articles")
            .WithName("Установка входящего количества в содержимое артикула");
    }
}