using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record MakeLinkageBetweenArticlesRequest(List<NewProductLinkageDto> Linkages);

public class MakeLinkageBetweenArticlesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/crosses",
                async (ISender sender, MakeLinkageBetweenArticlesRequest request, CancellationToken token) =>
                {
                    var command = request.Adapt<MakeLinkageBetweenProductsCommand>();
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