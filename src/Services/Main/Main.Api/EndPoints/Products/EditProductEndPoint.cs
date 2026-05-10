using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.PatchProduct;
using MediatR;

namespace Main.Api.EndPoints.Products;

public record EditProductRequest(PatchProductDto PatchProduct);

public class EditProductEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/products/{productId}",
                async (ISender sender, int productId, EditProductRequest request, CancellationToken token) =>
                {
                    var command = new PatchProductCommand(productId, request.PatchProduct);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).WithTags("Articles")
            .WithDescription("Редактирование артикула")
            .WithDisplayName("Редактирование артикула")
            .Accepts<EditProductRequest>(false, "application/json")
            .RequireAnyPermission("ARTICLES.EDIT");
    }
}