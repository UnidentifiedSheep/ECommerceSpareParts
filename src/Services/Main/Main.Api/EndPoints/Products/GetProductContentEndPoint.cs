using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductContent.GetProductContents;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record GetProductContentResponse(IReadOnlyList<ProductContentDto> Content);

public class GetProductContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/products/{productId}/contents",
                async (ISender sender, int productId, CancellationToken token) =>
                {
                    var result = await sender.Send(new GetProductContentsQuery(productId), token);
                    var response = new GetProductContentResponse(result.Contents);
                    return Results.Ok(response);
                }).WithName("получить содержание артикула")
            .WithTags("Articles")
            .WithDescription("Получить содержимое артикула по id.")
            .Produces<GetProductContentResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("получить содержание артикула");
    }
}