using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ArticleContent.RemoveArticleContent;
using MediatR;

namespace Main.Api.EndPoints.Articles;

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
                }).WithTags("Articles")
                .WithDescription("Удаление содержимого артикула в бд")
                .WithDisplayName("Удаление содержимого артикула")
                .RequireAnyPermission("ARTICLE.CONTENT.DELETE");
    }
}