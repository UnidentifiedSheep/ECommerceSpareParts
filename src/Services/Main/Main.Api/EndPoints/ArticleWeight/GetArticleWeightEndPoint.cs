using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Abstractions.Dtos.ArticleWeight;
using Main.Application.Handlers.ArticleWeight.GetArticleWeight;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleWeight;

public record GetArticleWeightResponse(ArticleWeightDto ArticleWeight);

public class GetArticleWeightEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{id:int}/weights", async (ISender sender, int id, CancellationToken token) =>
        {
            var result = await sender.Send(new GetArticleWeightQuery(id), token);
            var response = new GetArticleWeightResponse(result.ArticleWeight);
            return Results.Ok(response);
        }).WithTags("Article Weight")
        .WithDescription("Установка веса артикула.")
        .WithDisplayName("Установка веса артикула.")
        .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_GET);
    }
}