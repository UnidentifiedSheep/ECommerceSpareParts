using System.Text.Json.Serialization;
using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.GetFullProduct;
using MediatR;

namespace Main.Api.EndPoints.Internal.Products;

public record InternalGetFullProductResponse
{
    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }

    [JsonPropertyName("productWeight")]
    public ProductWeightDto? ProductWeight { get; init; }

    [JsonPropertyName("productSize")]
    public ProductSizeDto? ProductSize { get; init; }
}

public class InternalGetFullProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/internal/products/{id:int}/full", async (
                ISender sender,
                int id,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetFullProductQuery(id),
                    cancellationToken);

                return Results.Ok(new InternalGetFullProductResponse
                {
                    Product = result.Product,
                    ProductWeight = result.ProductWeight,
                    ProductSize = result.ProductSize
                });
            }).WithGroupName("Internal Products")
            .WithDisplayName("Internal service full product")
            .WithName("InternalFullProduct")
            .WithSummary("Получить полный продукт для внутреннего сервиса")
            .WithDescription("Получение продукта, веса и размеров для внутренних интеграций")
            .Produces<InternalGetFullProductResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
