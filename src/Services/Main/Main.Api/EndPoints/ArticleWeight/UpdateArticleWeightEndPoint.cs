using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ArticleWeight.SetArticleWeight;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleWeight;

public record UpdateArticleWeightRequest(decimal Weight, WeightUnit Unit);

public class UpdateArticleWeightEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/articles/{id:int}/weights",
            async (ISender sender, int id, UpdateArticleWeightRequest request, CancellationToken token) =>
            {
                var command = new SetArticleWeightCommand(id, request.Weight, request.Unit);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("Article Weight")
            .WithDescription("Обновление веса артикула.")
            .WithDisplayName("Обновление веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_UPDATE);
    }
}