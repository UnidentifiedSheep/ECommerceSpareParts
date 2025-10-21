using Carter;
using Main.Application.Handlers.Articles.SetArticleIndicator;
using MediatR;

namespace Main.Api.EndPoints.Articles;

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
            }).WithTags("Articles")
            .WithName("Установка индикатора артикула");
    }
}