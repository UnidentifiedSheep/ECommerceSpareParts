using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Anonymous.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleCharacteristics
{
	public record GetArticleCharacteristicsResponse(IEnumerable<CharacteristicsDto> Characteristics);
	public class GetArticleCharacteristicsEndPoint : ICarterModule
	{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
			app.MapGet("/articles/characteristics/{articleId}", async (ISender sender, int articleId, CancellationToken token) =>
			{
				var result = await sender.Send(new GetArticleCharacteristicsQuery(articleId), token);
				var response = result.Adapt<GetArticleCharacteristicsResponse>();
				return Results.Ok(response);
			}).WithName("Получение характеристик артикула по id")
			.WithGroup("Articles")
			  .WithDescription("Получить характеристики артикула")
			  .Produces<GetArticleCharacteristicsResponse>(StatusCodes.Status200OK)
			  .ProducesProblem(StatusCodes.Status400BadRequest)
			  .ProducesProblem(StatusCodes.Status404NotFound)
			  .WithSummary("Получение характеристик артикула по id");
		}
	}
}
