using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.EditArticle;

public record EditArticleRequest(PatchArticleDto PatchArticle);

public class EditArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/articles/{articleId}",
            async (ISender sender, int articleId, EditArticleRequest request, CancellationToken token) =>
            {
                var command = new EditArticleCommand(articleId, request.PatchArticle);
                await sender.Send(command, token);
                return Results.NoContent();
            }).RequireAuthorization("AMW")
            .WithGroup("Articles")
            .WithDescription("Редактирование артикула")
            .WithDisplayName("Редактирование артикула");
    }
}