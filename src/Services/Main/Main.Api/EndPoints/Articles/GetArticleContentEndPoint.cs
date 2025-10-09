using Main.Application.Handlers.ArticleContent.GetArticleContents;
using Carter;
using Core.Dtos.Anonymous.Articles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record GetArticleContentResponse(IEnumerable<ContentArticleDto> Content);

public class GetArticleContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId}/contents", async (ISender sender, int articleId, CancellationToken token) =>
            {
                var result = await sender.Send(new GetArticleContentsQuery(articleId), token);
                var response = result.Adapt<GetArticleContentResponse>();
                return Results.Ok(response);
            }).WithName("получить содержание артикула")
            .WithTags("Articles")
            .WithDescription("Получить содержимое артикула по id.")
            .Produces<GetArticleContentResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("получить содержание артикула");
    }
}