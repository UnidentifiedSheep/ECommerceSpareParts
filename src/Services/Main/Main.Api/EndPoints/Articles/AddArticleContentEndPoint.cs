using Carter;
using Main.Application.Handlers.ArticleContent.AddArticleContent;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record AddArticleContentRequest(Dictionary<int, int> Content);

public class AddArticleContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId}/contents", async (ISender sender, int articleId,
                AddArticleContentRequest request, CancellationToken cancellationToken) =>
            {
                var command = new AddArticleContentCommand(articleId, request.Content);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).RequireAuthorization("AMW")
            .WithTags("Articles")
            .WithDescription("Добавление содержимое артикула в бд")
            .WithDisplayName("Добавление содержимое артикула");
    }
}