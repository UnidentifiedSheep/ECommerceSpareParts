using Carter;
using Core.Dtos.Amw.Articles;
using Main.Application.Handlers.Articles.PatchArticle;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record EditArticleRequest(PatchArticleDto PatchArticle);

public class EditArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/articles/{articleId}",
                async (ISender sender, int articleId, EditArticleRequest request, CancellationToken token) =>
                {
                    var command = new PatchArticleCommand(articleId, request.PatchArticle);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).RequireAuthorization("AMW")
            .WithTags("Articles")
            .WithDescription("Редактирование артикула")
            .WithDisplayName("Редактирование артикула");
    }
}