using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.MakeLinkageBetweenArticles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record MakeLinkageBetweenProductsRequest(List<NewProductLinkageDto> Linkages);

public class MakeLinkageBetweenProductsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/crosses",
                async (ISender sender, MakeLinkageBetweenProductsRequest request, CancellationToken token) =>
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