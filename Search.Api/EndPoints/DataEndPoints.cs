using Mediator;
using Search.Abstractions.Dtos;
using Search.Application.Handler.Articles.AddArticle;
using Search.Application.Handler.Articles.GetArticle;

namespace Search.Api.EndPoints;

public record AddArticleRequest(ArticleDto Article);

public record GetArticleResponse(ArticleDto Article);

public static class DataEndPoints
{
    public static IEndpointRouteBuilder MapDataEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/data");

        group.MapGet("/articles/{id:int}", GetArticle);
        group.MapPost("/articles", AddArticle);
        
        return routes;
    }

    static async Task<IResult> GetArticle(int id, IMediator mediator, CancellationToken cancellationToken)
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