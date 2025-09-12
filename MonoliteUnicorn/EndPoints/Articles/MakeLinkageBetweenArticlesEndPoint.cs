using Application.Handlers.Articles.MakeLinkageBetweenArticles;
using Carter;
using Core.Dtos.Amw.Articles;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Articles;

public record MakeLinkageBetweenArticlesRequest(NewArticleLinkageDto Linkage);

public class MakeLinkageBetweenArticlesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/crosses", async (ISender sender, MakeLinkageBetweenArticlesRequest request, CancellationToken token) =>
        {
            var command = request.Adapt<MakeLinkageBetweenArticlesCommand>();
            await sender.Send(command, token);
            return Results.Ok();
        }).RequireAuthorization("AMW")
        .WithTags("Articles")
        .WithDescription("Создание кроссировки между артикулами")
        .WithDisplayName("Создание кроссировки");
    }
}