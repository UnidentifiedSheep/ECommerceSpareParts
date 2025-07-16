using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles.SetArticleIndicator;

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
            .WithGroup("Articles")
            .WithName("Установка индикатора артикула");
    }
}