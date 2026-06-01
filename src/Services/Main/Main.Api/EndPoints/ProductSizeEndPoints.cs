using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductSizes.DeleteProductSizes;
using Main.Application.Handlers.ProductSizes.GetProductSizes;
using Main.Application.Handlers.ProductSizes.SetProductSizes;
using MediatR;

namespace Main.Api.EndPoints;

public record GetProductSizeResponse(ProductSizeDto ProductSize);

public record PutProductSizeRequest(decimal Length, decimal Width, decimal Height, DimensionUnit Unit);

public class ProductSizeEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var sizes = app.MapGroup("/products/{id:int}/sizes")
            .WithTags("Product Size");

        sizes.MapDelete("", async (ISender sender, int id, CancellationToken token) =>
            {
                await sender.Send(new DeleteArticleSizesCommand(id), token);
                return Results.NoContent();
            })
            .WithName("DeleteProductSize")
            .WithSummary("Удалить размеры продукта")
            .WithDescription("Удаление размеров артикула.")
            .WithDisplayName("Удаление размеров артикула.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_DELETE);

        sizes.MapGet("", async (ISender sender, int id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetProductSizeQuery(id), token);
                return Results.Ok(new GetProductSizeResponse(result.ProductSize));
            })
            .WithName("GetProductSize")
            .WithSummary("Получить размеры продукта")
            .WithDescription("Получение размеров артикула.")
            .WithDisplayName("Получение размеров артикула.")
            .Produces<GetProductSizeResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_GET);

        sizes.MapPut("", async (
                ISender sender,
                int id,
                PutProductSizeRequest request,
                CancellationToken token) =>
            {
                var command = new SetProductSizesCommand(id, request.Length, request.Width, request.Height, request.Unit);
                await sender.Send(command, token);
                return Results.Created();
            })
            .WithName("SetProductSize")
            .WithSummary("Установить размеры продукта")
            .WithDescription("Установка размеров артикула.")
            .WithDisplayName("Установка размеров артикула.")
            .Accepts<PutProductSizeRequest>(false, "application/json")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_CREATE);
    }
}
