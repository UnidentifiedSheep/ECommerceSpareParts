using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ArticleWeight.SetArticleWeight;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleWeight;

public record CreateArticleWeightRequest(decimal Weight, WeightUnit Unit);

public class CreateArticleWeightEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{id:int}/weights", 
            async (ISender sender, int id, CreateArticleWeightRequest request, CancellationToken token) =>
        {
            var command = new SetArticleWeightCommand(id, request.Weight, request.Unit);
            await sender.Send(command, token);
            return Results.Created();
        }).WithTags("Article Weight")
            .WithDescription("Установка веса артикула.")
            .WithDisplayName("Установка веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_CREATE);
    }
}