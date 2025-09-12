using Application.Handlers.Articles.CreateArticles;
using Carter;
using Core.Dtos.Services.Articles;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles;

public record CreateArticleRequest(List<CreateArticleDto> NewArticles);

public class CreateArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles", async (ISender sender, CreateArticleRequest request, CancellationToken token) =>
        {
            var command = request.Adapt<CreateArticlesCommand>();
            await sender.Send(command, token);
            return Results.Created();
        }).RequireAuthorization("AMW")
            .WithTags("Articles")
            .WithDescription("Добавление новых артикулов в бд")
            .WithDisplayName("Добавление артикулов");
    }
}