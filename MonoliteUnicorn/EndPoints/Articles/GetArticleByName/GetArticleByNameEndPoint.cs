using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleByName;

public record GetArticleByNameResponse(IEnumerable<ArticleDto> Articles);

public class GetArticleByNameEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/search/name", async (ISender sender, string searchTerm, int? page, int? viewCount,
            string? sortBy, ClaimsPrincipal user, HttpContext context, CancellationToken token) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var producerIds = context.Request.Query["producerId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();
            var query = new GetArticleByNameQuery(searchTerm, page ?? 0, viewCount ?? 24, sortBy, producerIds, userId);
            var result = await sender.Send(query, token);
            var response = result.Adapt<GetArticleByNameResponse>();
            return Results.Ok(response);
        }).WithGroup("Articles")
        .WithDescription("Поиск артикулов по названию")
        .WithDisplayName("Поиск артикулов по названию");
    }
}