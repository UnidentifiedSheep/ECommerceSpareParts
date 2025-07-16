using Carter;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Anonymous.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticlePairs;


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
        });
    }
}