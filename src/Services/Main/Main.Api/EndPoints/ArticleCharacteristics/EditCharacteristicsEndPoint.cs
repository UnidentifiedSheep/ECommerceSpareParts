using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ArticleCharacteristics.PatchCharacteristics;
using Main.Core.Dtos.Amw.ArticleCharacteristics;
using MediatR;

namespace Main.Api.EndPoints.ArticleCharacteristics;

public record EditCharacteristicsRequest(PatchCharacteristicsDto Value);

public class EditCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/articles/characteristics/{id:int}", 
            async (ISender sender, int id, EditCharacteristicsRequest request, CancellationToken token) =>
            {
                var command = new PatchCharacteristicsCommand(id, request.Value);
                await sender.Send(command, token);
                return Results.Ok();
            }).WithName("Редактирование характеристики артикула по id")
            .WithTags("Article Characteristics")
            .WithDescription("Редактирование характеристики")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Редактирование характеристики")
            .RequireAnyPermission("ARTICLE.CHARACTERISTICS.UPDATE");
    }
}