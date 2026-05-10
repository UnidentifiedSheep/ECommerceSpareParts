using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductCharacteristics.PatchCharacteristics;
using MediatR;

namespace Main.Api.EndPoints.ProductCharacteristics;

public record EditCharacteristicsRequest(PatchCharacteristicsDto Value);

public class EditCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/products/{productId}/characteristics/{name}",
                async (
                    ISender sender,
                    int productId,
                    string name,
                    EditCharacteristicsRequest request,
                    CancellationToken token) =>
                {
                    var command = new PatchCharacteristicsCommand(productId, name, request.Value);
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