using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaExecNumber;

public record GetArticlesViaExecResponse(IEnumerable<ArticleDto> Articles);
public class GetArticleViaExecNumberEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/amw/articles/search/exec/{article}", async (ISender sender, HttpContext context, string article, int viewCount, int page, string? sortBy, CancellationToken token) => 
        {
            var producerIds = context.Request.Query["producerId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();
            var result = await sender.Send(new GetArticleViaExecNumberQuery(article, viewCount, page, sortBy, producerIds), token);
            var response = result.Adapt<GetArticlesViaExecResponse>();
            return Results.Ok(response); 
        }).RequireAuthorization("AMW")
            .WithGroup("Articles")
            .WithDescription("Поиск артикула по точному номеру")
            .Produces<GetArticlesViaExecResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Поиск артикула по точному номеру");
    }
}