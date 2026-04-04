using Carter;
using MediatR;
using Search.Abstractions.Dtos;
using Search.Application.Handler.Articles.AddArticle;

namespace Search.Api.EndPoints.Articles;

public record AddArticleRequest(ArticleDto Article);

public class CreateArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/", 
                async (ISender sender, AddArticleRequest request, CancellationToken cancellationToken) => 
                { 
                    await sender.Send(new AddArticleCommand(request.Article), cancellationToken); 
                    return Results.Created(); 
                })
            .WithName("CreateArticle")
            .WithDescription("Creates a new article")
            .Produces(StatusCodes.Status201Created);
    }
}