using Api.Common.Models.Requests;
using Carter;
using Main.Application.Dtos.Anonymous.Articles;
using Main.Application.Handlers.ProductCharacteristics.GetCharacteristics;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.ArticleCharacteristics;

public record GetProductCharacteristicsResponse(IReadOnlyList<ProductCharacteristicDto> Characteristics);

public class GetCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{productId:int}/characteristics",
                async (
                    ISender sender,
                    int productId,
                    [AsParameters] PaginationQueryModel queryParams,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetCharacteristicsQuery(productId, queryParams),
                        token);
                    var response = result.Adapt<GetProductCharacteristicsResponse>();
                    return Results.Ok(response);
                }).WithName("Получение характеристик артикула по id")
            .WithTags("Article Characteristics")
            .WithDescription("Получить характеристики артикула")
            .Produces<GetProductCharacteristicsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Получение характеристик артикула по id");
    }
}