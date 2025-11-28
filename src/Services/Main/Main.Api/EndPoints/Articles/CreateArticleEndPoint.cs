using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Articles.CreateArticles;
using Main.Core.Dtos.Services.Articles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record CreateArticleRequest(List<CreateArticleDto> NewArticles);

public record CreateArticleResponse(List<int> CreatedIds);
public class CreateArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles", async (ISender sender, CreateArticleRequest request, CancellationToken token) =>
            {
                var command = request.Adapt<CreateArticlesCommand>();
                var result = await sender.Send(command, token);
                var response = result.Adapt<CreateArticleResponse>();
                return Results.Created("/articles", response);
            }).WithTags("Articles")
            .WithDescription("Добавление новых артикулов")
            .WithDisplayName("Добавление артикулов")
            .Accepts<CreateArticleRequest>(false, "application/json")
            .Produces<CreateArticleResponse>(201, "application/json")
            .ProducesProblem(400)
            .RequireAnyPermission("ARTICLES.CREATE");
    }
}