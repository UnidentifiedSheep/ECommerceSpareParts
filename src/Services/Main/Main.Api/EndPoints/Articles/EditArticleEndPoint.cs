using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Application.Handlers.Articles.PatchArticle;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record EditArticleRequest(PatchProductDto PatchProduct);

public class EditArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/articles/{articleId}",
                async (ISender sender, int articleId, EditArticleRequest request, CancellationToken token) =>
                {
                    var command = new PatchArticleCommand(articleId, request.PatchProduct);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).WithTags("Articles")
            .WithDescription("Редактирование артикула")
            .WithDisplayName("Редактирование артикула")
            .Accepts<EditArticleRequest>(false, "application/json")
            .RequireAnyPermission("ARTICLES.EDIT");
    }
}