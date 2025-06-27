using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaStartNumber;

public record GetArticleViaStartNumberResponse(IEnumerable<ArticleDto> Articles);
public class GetArticleViaStartNumberEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/search/start", async (ISender sender, HttpContext context, ClaimsPrincipal user, string searchTerm, int viewCount, int page, string? sortBy, CancellationToken token) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var producerIds = context.Request.Query["producerId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();
            
            var command = new GetArticleViaStartNumberQuery(searchTerm, viewCount, page, sortBy, producerIds, userId);
            var result = await sender.Send(command, token);
            var response = result.Adapt<GetArticleViaStartNumberResponse>();
            return Results.Ok(response);    
        }).WithGroup("Articles")
            .WithDescription("Поиск артикула с начала номера")
            .Produces<GetArticleViaStartNumberResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Поиск артикула с начала номера");
    }
}