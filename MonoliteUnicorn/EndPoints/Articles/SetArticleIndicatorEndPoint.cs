using Application.Handlers.Articles.SetArticleIndicator;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles;

public record SetArticleIndicatorRequest(string? Indicator);

public class SetArticleIndicatorEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/articles/{articleId}/indicator", async (ISender sender, int articleId, 
            SetArticleIndicatorRequest request, CancellationToken token) =>
        {
            var command = new SetArticleIndicatorCommand(articleId, request.Indicator);
            await sender.Send(command, token);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
            .WithTags("Articles")
            .WithName("Установка индикатора артикула");
    }
}