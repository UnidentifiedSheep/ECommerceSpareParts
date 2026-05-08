using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ProductCharacteristics.DeleteCharacteristics;
using MediatR;

namespace Main.Api.EndPoints.ArticleCharacteristics;

public class DeleteCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/products/{productId}/characteristics/{name}",
                async (ISender sender, int productId, string name, CancellationToken token) =>
                {
                    await sender.Send(new DeleteCharacteristicsCommand(productId, name), token);
                    return Results.Ok();
                }).WithName("Удаление характеристики артикула по id")
            .WithTags("Article Characteristics")
            .WithDescription("Удаление характеристики")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Удаление характеристики")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE);
    }
}