using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.GetProductPair;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record GetProductPairResponse(ProductDto? Pair);

public class GetProductPairEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/products/{productId}/pairs",
                async (ISender sender, int productId, CancellationToken token) =>
                {
                    var query = new GetProductPairQuery(productId);
                    var result = await sender.Send(query, token);
                    return Results.Ok(new GetProductPairResponse(result.Pair));
                }).WithTags("Articles")
            .WithDescription("Поиск пар артикула")
            .WithSummary("Поиск пар артикула");
    }
}