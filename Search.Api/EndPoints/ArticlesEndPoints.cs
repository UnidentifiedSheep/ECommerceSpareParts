using Mediator;
using Search.Abstractions.Dtos;
using Search.Application.Handler.Articles.AddArticle;
using Search.Application.Handler.Articles.GetArticle;
using Search.Application.Handler.Articles.SearchArticles;
using Search.Enums;

namespace Search.Api.EndPoints;

public record AddArticleRequest(ArticleDto Article);

public record GetArticleResponse(ArticleDto Article);
public record SearchArticlesResponse(IReadOnlyList<ArticleDto> Articles, string? Cursor);

public static class ArticlesEndPoints
{
    public static IEndpointRouteBuilder MapDataEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/articles");

        group.MapPost("/", AddArticle);
        group.MapGet("/{id:int}", GetArticleById);
        group.MapGet("/", SearchArticles);
        
        return routes;
    }

    static async Task<IResult> SearchArticles(string query, string? cursor, int limit, ArticleSearchVariant searchVariant, 
        IMediator mediator, CancellationToken cancellationToken)
    {
        SearchArticlesQuery q = new SearchArticlesQuery(query, cursor, limit, searchVariant);
        var result = await mediator.Send(q, cancellationToken);
        return Results.Ok(new SearchArticlesResponse(result.Articles, result.Cursor));
    }

    static async Task<IResult> GetArticleById(int id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetArticleQuery(id), cancellationToken);
        return Results.Ok(new GetArticleResponse(result.Article));
    }

    static async Task<IResult> AddArticle(AddArticleRequest request, IMediator mediator, CancellationToken cancellationToken)
    {
        await mediator.Send(new AddArticleCommand(request.Article), cancellationToken);
        return Results.Created();
    }
}