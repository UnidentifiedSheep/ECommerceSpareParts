using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleImgs;

public record GetArticleImgsResponse(Dictionary<int, HashSet<string>> ArticleImages); 

public class GetArticleImgsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/imgs", async (ISender sender, HttpContext context, CancellationToken token) =>
        {
            var articleIds = context.Request.Query["articleId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();
            var query = new GetArticleImgsQuery(articleIds);
            var result = await sender.Send(query, token);
            var response = result.Adapt<GetArticleImgsResponse>();
            return Results.Ok(response);
        }).WithGroup("Articles")
        .WithName("Get Images of article")
        .WithDescription("Получить изображения артикула");
    }
}