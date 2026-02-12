using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ArticleWeight.DeleteArticleWeight;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleWeight;

public class DeleteArticleWeightEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/{id:int}/weights", async (ISender sender, int id, CancellationToken token) => 
            {
                var command = new DeleteArticleWeightCommand(id);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("Article Weight")
            .WithDescription("Удаление веса артикула.")
            .WithDisplayName("Удаление веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_DELETE);
    }
}