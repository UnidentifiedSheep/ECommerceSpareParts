using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles.AddArticlesContent;

public record AddArticleContentRequest(int ArticleId, Dictionary<int, int> Content);

public class AddArticleContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId}/contents", async (ISender sender, int articleId, 
                AddArticleContentRequest request, CancellationToken cancellationToken) =>
        {
            var command = new AddArticleContentCommand(request.ArticleId, request.Content);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
        .WithGroup("Articles")
        .WithDescription("Добавление содержимое артикула в бд")
        .WithDisplayName("Добавление содержимое артикула");
    }
}