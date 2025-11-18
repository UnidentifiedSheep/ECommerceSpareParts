using Carter;
using Main.Application.Handlers.Articles.CreateArticles;
using Main.Core.Dtos.Services.Articles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

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
            }).WithTags("Articles")
            .WithDescription("Добавление новых артикулов")
            .WithDisplayName("Добавление артикулов")
            .Accepts<CreateArticleRequest>(false, "application/json");
    }
}