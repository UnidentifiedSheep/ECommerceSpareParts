using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Articles;

namespace MonoliteUnicorn.EndPoints.Articles.MakeLinkageBetweenArticles;

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
        .WithGroup("Articles")
        .WithDescription("Создание кроссировки между артикулами")
        .WithDisplayName("Создание кроссировки");
    }
}