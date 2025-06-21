using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.CreateArticle;

public record CreateArticleRequest(List<NewArticleDto> NewArticles);

public class CreateArticleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles", async (ISender sender, CreateArticleRequest request, CancellationToken token) =>
        {
            var command = request.Adapt<CreateArticleCommand>();
            await sender.Send(command, token);
            return Results.Ok();
        }).RequireAuthorization("AMW")
            .WithGroup("Articles")
            .WithDescription("Добавление новых артикулов в бд")
            .WithDisplayName("Добавление артикулов");
    }
}