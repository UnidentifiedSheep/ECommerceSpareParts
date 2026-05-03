using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record CreateProductRequest(List<CreateProductDto> NewProducts);

public record CreateProductResponse(IReadOnlyList<int> CreatedIds);

public class CreateProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (ISender sender, CreateProductRequest request, CancellationToken token) =>
            {
                var command = new CreateProductsCommand(request.NewProducts);
                var result = await sender.Send(command, token);
                var response = new CreateProductResponse(result.CreatedIds);
                return Results.Created("/products", response);
            }).WithTags("Articles")
            .WithDescription("Добавление новых артикулов")
            .WithDisplayName("Добавление артикулов")
            .Accepts<CreateProductRequest>(false, "application/json")
            .Produces<CreateProductResponse>(201, "application/json")
            .ProducesProblem(400)
            .RequireAnyPermission("ARTICLES.CREATE");
    }
}