using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Anonymous.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleContent
{
	public record GetArticleContentResponse(IEnumerable<ContentArticleDto> Content);
	public class GetArticleContentEndPoint : ICarterModule
	{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
			app.MapGet("/articles/{articleId}/contents", async (ISender sender, int articleId, CancellationToken token) =>
			{
				var result = await sender.Send(new GetArticleContentQuery(articleId), token);
				var response = result.Adapt<GetArticleContentResponse>();
				return Results.Ok(response);
			}).WithName("получить содержание артикула")
			.WithGroup("Articles")
			  .WithDescription("Получить содержимое артикула по id.")
			  .Produces<GetArticleContentResponse>(StatusCodes.Status200OK)
			  .ProducesProblem(StatusCodes.Status400BadRequest)
			  .WithSummary("получить содержание артикула");
		}
	}
}
