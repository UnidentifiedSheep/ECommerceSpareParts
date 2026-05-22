using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductContent.AddProductContent;
using Main.Application.Handlers.ProductContent.GetProductContents;
using Main.Application.Handlers.ProductContent.RemoveProductContent;
using Main.Application.Handlers.ProductContent.SetProductContentQuantity;
using MediatR;

namespace Main.Api.EndPoints.Products;

public record AddProductContentRequest(Dictionary<int, int> Content);

public record GetProductContentResponse(IReadOnlyList<ProductContentDto> Content);

public record SetProductsContentCountRequest(int Count);

public static class ProductContentEndPoints
{
    public static RouteGroupBuilder MapProductContentEndPoints(this RouteGroupBuilder products)
    {
        products.MapGet("/{productId}/contents", async (
                ISender sender,
                int productId,
                CancellationToken token) =>
            {
                var result = await sender.Send(new GetProductContentsQuery(productId), token);
                var response = new GetProductContentResponse(result.Contents);
                return Results.Ok(response);
            })
            .WithName("получить содержание артикула")
            .WithDescription("Получить содержимое артикула по id.")
            .Produces<GetProductContentResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("получить содержание артикула");

        products.MapPost("/{productId}/contents", async (
                ISender sender,
                int productId,
                AddProductContentRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new AddProductContentCommand(productId, request.Content);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .WithDescription("Добавление содержимого артикула")
            .WithDisplayName("Добавление содержимого артикула")
            .Accepts<AddProductContentRequest>(false, "application/json")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CONTENT_CREATE);

        products.MapDelete("/{productId}/contents/{childProductId}", async (
                ISender sender,
                int productId,
                int childProductId,
                CancellationToken token) =>
            {
                var command = new RemoveProductContentCommand(productId, childProductId);
                await sender.Send(command, token);
                return Results.NoContent();
            })
            .WithDescription("Удаление содержимого артикула в бд")
            .WithDisplayName("Удаление содержимого артикула")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CONTENT_DELETE);

        products.MapPatch("/{productId}/contents/{childProductId}", async (
                ISender sender,
                int productId,
                int childProductId,
                SetProductsContentCountRequest request,
                CancellationToken token) =>
            {
                var command = new SetProductsContentCountCommand(productId, childProductId, request.Count);
                await sender.Send(command, token);
                return Results.NoContent();
            })
            .WithName("Установка входящего количества в содержимое артикула")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CONTENT_EDIT);

        return products;
    }
}
