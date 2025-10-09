using Carter;
using Core.Dtos.Anonymous.Articles;
using Main.Application.Handlers.ArticlePairs.GetArticlePair;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record GetArticlePairsResponse(IEnumerable<ArticleDto> Pairs);

public class GetArticlePairsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId}/pairs", async (ISender sender, int articleId, CancellationToken token) =>
            {
                var query = new GetArticlePairsQuery(articleId);
                var result = await sender.Send(query, token);
                return Results.Ok(result.Adapt<GetArticlePairsResponse>());
            }).WithTags("Articles")
            .WithDescription("Поиск пар артикула")
            .WithSummary("Поиск пар артикула");
    }
}