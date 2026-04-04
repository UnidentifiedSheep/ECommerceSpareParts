using Carter;
using MediatR;
using Search.Abstractions.Dtos;
using Search.Application.Handler.Articles.GetArticle;

namespace Search.Api.EndPoints.Articles;

public record GetArticleResponse(ArticleDto Article);

public class GetArticleByIdEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{id}", async (int id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetArticleQuery(id), cancellationToken);
            return Results.Ok(new GetArticleResponse(result.Article));
        }).WithName("GetArticleById")
        .WithDescription("Gets an article by id.")
        .Produces<GetArticleResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}