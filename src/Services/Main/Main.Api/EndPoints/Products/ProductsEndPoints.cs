using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Application.Handlers.Products.PatchProduct;
using MediatR;

namespace Main.Api.EndPoints.Products;

public record CreateProductRequest(List<CreateProductDto> NewProducts);

public record CreateProductResponse(IReadOnlyList<int> CreatedIds);

public record EditProductRequest(PatchProductDto PatchProduct);

public class ProductsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products")
            .WithTags("Product");

        products.MapProductContentEndPoints();
        products.MapProductRelationsEndPoints();
        products.MapProductImagesEndPoints();

        products.MapPost("", async (
                ISender sender,
                CreateProductRequest request,
                CancellationToken token) =>
            {
                var command = new CreateProductsCommand(request.NewProducts);
                var result = await sender.Send(command, token);
                var response = new CreateProductResponse(result.CreatedIds);
                return Results.Created("/products", response);
            })
            .WithDescription("Добавление новых артикулов")
            .WithDisplayName("Добавление артикулов")
            .Accepts<CreateProductRequest>(false, "application/json")
            .Produces<CreateProductResponse>(201, "application/json")
            .ProducesProblem(400)
            .RequireAnyPermission(PermissionCodes.ARTICLES_CREATE);

        products.MapPatch("/{productId}", async (
                ISender sender,
                int productId,
                EditProductRequest request,
                CancellationToken token) =>
            {
                var command = new PatchProductCommand(productId, request.PatchProduct);
                await sender.Send(command, token);
                return Results.NoContent();
            })
            .WithDescription("Редактирование артикула")
            .WithDisplayName("Редактирование артикула")
            .Accepts<EditProductRequest>(false, "application/json")
            .RequireAnyPermission(PermissionCodes.ARTICLES_EDIT);
    }
}
