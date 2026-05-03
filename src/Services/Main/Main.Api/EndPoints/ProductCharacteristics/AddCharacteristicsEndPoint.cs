using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Amw.ArticleCharacteristics;
using Main.Application.Handlers.ProductCharacteristics.AddCharacteristics;
using MediatR;

namespace Main.Api.EndPoints.ArticleCharacteristics;

public record AddCharacteristicsRequest(IEnumerable<NewCharacteristicsDto> Characteristics);
public class AddCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/characteristics/",
                async (ISender sender, AddCharacteristicsRequest request, CancellationToken token) =>
                {
                    await sender.Send(new AddCharacteristicsCommand(request.Characteristics), token);
                    return Results.Created();
                }).WithName("Создание характеристик артикула")
            .WithTags("Article Characteristics")
            .WithDescription("Создание характеристик артикула")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Создание характеристик артикула")
            .RequireAnyPermission("ARTICLE.CHARACTERISTICS.CREATE");
    }
}