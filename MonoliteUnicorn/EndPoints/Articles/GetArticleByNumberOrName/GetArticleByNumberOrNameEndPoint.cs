using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleByNumberOrName;

public record GetArticleByNumberOrNameResponse(IEnumerable<ArticleDto> Articles);

public class GetArticleByNumberOrNameEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/search", 
            async (ISender sender, string searchTerm, int? page, int? viewCount, string? sortBy, HttpContext context, CancellationToken token) =>
        {
            var producerIds = context.Request.Query["producerId"]
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();
            var query = new GetArticleByNumberOrNameQuery(searchTerm, page ?? 0, viewCount ?? 24, sortBy, producerIds);
            var result = await sender.Send(query, token);
            var response = result.Adapt<GetArticleByNumberOrNameResponse>();
            return Results.Ok(response);
        }).WithGroup("Articles")
            .WithDescription("Поиск артикулов по всем параметрам, Название, номер итд.")
            .WithDisplayName("Поиск по всем критериям");
    }
}