using Carter;
using Main.Application.Handlers.ArticleCharacteristics.GetCharacteristics;
using Main.Core.Dtos.Anonymous.Articles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record GetArticleCharacteristicsResponse(IEnumerable<CharacteristicsDto> Characteristics);

public class GetArticleCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/characteristics/{articleId}",
                async (ISender sender, int articleId, CancellationToken token) =>
                {
                    var result = await sender.Send(new GetArticleCharacteristicsQuery(articleId), token);
                    var response = result.Adapt<GetArticleCharacteristicsResponse>();
                    return Results.Ok(response);
                }).WithName("Получение характеристик артикула по id")
            .WithTags("Articles")
            .WithDescription("Получить характеристики артикула")
            .Produces<GetArticleCharacteristicsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получение характеристик артикула по id");
    }
}