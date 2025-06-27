using System.Security.Claims;
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
        app.MapGet("/articles/search/exec/{article}", async (ISender sender, HttpContext context, ClaimsPrincipal user, string article, int viewCount, int page, string? sortBy, CancellationToken token) => 
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var producerIds = context.Request.Query["producerId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();
            
            var query = new GetArticleViaExecNumberQuery(article, viewCount, page, sortBy, producerIds, userId);
            var result = await sender.Send(query, token);
            var response = result.Adapt<GetArticlesViaExecResponse>();
            return Results.Ok(response); 
        }).WithGroup("Articles")
            .WithDescription("Поиск артикула по точному номеру")
            .Produces<GetArticlesViaExecResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Поиск артикула по точному номеру");
    }
}