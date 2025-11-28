using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ArticleCharacteristics.DeleteCharacteristics;
using MediatR;

namespace Main.Api.EndPoints.ArticleCharacteristics;

public class DeleteCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/characteristics/{id:int}", async (ISender sender, int id, CancellationToken token) =>
        {
            await sender.Send(new DeleteCharacteristicsCommand(id), token);
            return Results.Ok();
        }).WithName("Удаление характеристики артикула по id")
        .WithTags("Article Characteristics")
        .WithDescription("Удаление характеристики")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Удаление характеристики")
        .RequireAnyPermission("ARTICLE.CHARACTERISTICS.DELETE");
    }
}