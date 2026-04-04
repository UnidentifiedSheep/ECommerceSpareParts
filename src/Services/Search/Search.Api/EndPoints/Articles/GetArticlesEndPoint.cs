using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Search.Abstractions.Dtos;
using Search.Application.Handler.Articles.SearchArticles;
using Search.Enums;

namespace Search.Api.EndPoints.Articles;

public record GetArticleRequest
{
    [FromQuery(Name = "query")]
    public string Query { get; set; } = string.Empty;
    
    [FromQuery(Name = "cursor")]
    public string? Cursor { get; set; }
    
    [FromQuery(Name = "limit")]
    public int Limit { get; set; }
    
    [FromQuery(Name = "searchVariant")]
    public ArticleSearchVariant SearchVariant { get; set; }
}

public record SearchArticlesResponse(IReadOnlyList<ArticleDto> Articles, string? Cursor);

public class GetArticlesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/", async (
                [AsParameters] GetArticleRequest request,
                ISender mediator,
                CancellationToken cancellationToken) =>
            {
                var q = new SearchArticlesQuery(request.Query, request.Cursor, request.Limit, request.SearchVariant);
                var result = await mediator.Send(q, cancellationToken);
                return Results.Ok(new SearchArticlesResponse(result.Articles, result.Cursor));
            }).WithName("GetArticles")
            .WithDescription("Gets articles by query")
            .Produces(StatusCodes.Status200OK);
    }
}