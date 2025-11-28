using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;
using Main.Core.Dtos.Amw.Articles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record MakeLinkageBetweenArticlesRequest(List<NewArticleLinkageDto> Linkages);

public class MakeLinkageBetweenArticlesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/crosses",
                async (ISender sender, MakeLinkageBetweenArticlesRequest request, CancellationToken token) =>
                {
                    var command = request.Adapt<MakeLinkageBetweenArticlesCommand>();
                    await sender.Send(command, token);
                    return Results.Created();
                }).WithTags("Articles")
                .WithDescription("Создание кроссировки между артикулами")
                .WithDisplayName("Создание кроссировки")
                .Produces(201)
                .ProducesProblem(404)
                .ProducesProblem(400)
                .RequireAnyPermission("ARTICLE.CROSSES.CREATE");
    }
}