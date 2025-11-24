using Carter;
using Main.Application.Handlers.ArticleCharacteristics.GetCharacteristics;
using Main.Core.Dtos.Anonymous.Articles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.ArticleCharacteristics;

public record GetArticleCharacteristicsResponse(IEnumerable<CharacteristicsDto> Characteristics);

public class GetCharacteristicsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId:int}/characteristics/",
                async (ISender sender, int articleId, HttpContext context, CancellationToken token) =>
                {
                    var characteristicsIds = context.Request.Query["id"]
                        .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();
                    var result = await sender.Send(new GetArticleCharacteristicsQuery(articleId, characteristicsIds), token);
                    var response = result.Adapt<GetArticleCharacteristicsResponse>();
                    return Results.Ok(response);
                }).WithName("Получение характеристик артикула по id")
            .WithTags("Article Characteristics")
            .WithDescription("Получить характеристики артикула")
            .Produces<GetArticleCharacteristicsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Получение характеристик артикула по id");
    }
}